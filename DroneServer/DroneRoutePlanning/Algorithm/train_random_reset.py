import math
import random
from itertools import count

import matplotlib
import matplotlib.pyplot as plt
import torch
import torch.nn as nn
import torch.optim as optim

from DroneRoutePlanning.Algorithm.DQN import DQN, ReplayMemory, Transition
from DroneRoutePlanning.Algorithm.DroneEnvironment import DroneEnvironment


def select_action(_state):
    global steps_done
    sample = random.random()
    # 计算epsilon阈值，随着步数的增加，epsilon逐渐减小
    eps_threshold = EPS_END + (EPS_START - EPS_END) * math.exp(-1. * steps_done / EPS_DECAY)
    steps_done += 1
    # 以epsilon的概率选择随机动作
    if sample > eps_threshold:
        with torch.no_grad():
            # 否则选择使得Q值最大的动作
            return policy_net(_state).max(1).indices.view(1, 1)
    else:
        return torch.tensor([[env.action_space.sample()]], device=device, dtype=torch.long)


def plot_durations(show_result=False, update_interval=10):
    # 每隔一定的间隔或者训练结束时更新图像
    if len(episode_durations) % update_interval != 0 and not show_result:
        return

    plt.figure(1)
    durations_t = torch.tensor(episode_durations, dtype=torch.float)
    if show_result:
        plt.title('Result')
    else:
        plt.clf()
        plt.title('Training...')
    plt.xlabel('Episode')
    plt.ylabel('Duration')
    plt.plot(durations_t.numpy())
    # 计算并绘制每100个episode的平均持续时间
    if len(durations_t) >= 100:
        means = durations_t.unfold(0, 100, 1).mean(1).view(-1)
        means = torch.cat((torch.zeros(99), means))
        plt.plot(means.numpy())

    plt.pause(0.001)

    if is_ipython:
        if not show_result:
            display.display(plt.gcf())
            display.clear_output(wait=True)
        else:
            display.display(plt.gcf())


def optimize_model():
    # 如果记忆库中的转换数量小于批量大小，则不进行优化
    if len(memory) < BATCH_SIZE:
        return
    transitions = memory.sample(BATCH_SIZE)
    # 将批量转换转置以便我们可以批量处理它们
    batch = Transition(*zip(*transitions))

    # 计算非最终状态的掩码，并连接批处理元素
    non_final_mask = torch.tensor(tuple(map(lambda s: s is not None,
                                            batch.next_state)), device=device, dtype=torch.bool)
    non_final_next_states = torch.cat([s for s in batch.next_state
                                       if s is not None])
    state_batch = torch.cat(batch.state)
    action_batch = torch.cat(batch.action)
    reward_batch = torch.cat(batch.reward)

    # 计算Q(s_t, a)，然后我们选择已经采取的动作的列
    state_action_values = policy_net(state_batch).gather(1, action_batch)

    # 计算所有下一个状态的V(s_{t+1})
    next_state_values = torch.zeros(BATCH_SIZE, device=device)
    with torch.no_grad():
        next_state_values[non_final_mask] = target_net(non_final_next_states).max(1).values
    # 计算期望的Q值
    expected_state_action_values = (next_state_values * GAMMA) + reward_batch

    # 计算Huber损失
    criterion = nn.SmoothL1Loss()
    loss = criterion(state_action_values, expected_state_action_values.unsqueeze(1))

    # 优化模型
    optimizer.zero_grad()
    loss.backward()
    # 对梯度进行裁剪
    torch.nn.utils.clip_grad_value_(policy_net.parameters(), 100)
    optimizer.step()


if __name__ == '__main__':
    # 设置matplotlib
    is_ipython = 'inline' in matplotlib.get_backend()
    if is_ipython:
        from IPython import display

    plt.ion()

    # 使用GPU
    device = torch.device(
        "cuda" if torch.cuda.is_available() else
        "mps" if torch.backends.mps.is_available() else
        "cpu"
    )

    print(f"Using {device} device")

    # 初始化环境
    env = DroneEnvironment(drone_position=[0, 0, 0],
                           person_position_in_camera=[200, 140, 220, 150],
                           obstacle_positions=[5, 5, 5],
                           screen_size=[552, 326],
                           detect_obstacle_distance=10,
                           max_steps=1000)

    reset_policy = env.random_reset

    # 设置训练参数
    BATCH_SIZE = 128
    GAMMA = 0.99
    EPS_START = 0.01
    EPS_END = 0.001
    EPS_DECAY = 100
    TAU = 5e-5
    LR = 1e-6

    # 获取动作空间的大小和状态观测的数量
    n_actions = env.action_space.n
    state, info = reset_policy()
    n_observations = len(state)

    # 初始化网络
    policy_net = DQN(n_observations, n_actions).to(device)
    target_net = DQN(n_observations, n_actions).to(device)
    policy_net.load_state_dict(torch.load('models/sim_reset.pth', weights_only=True))
    target_net.load_state_dict(policy_net.state_dict())

    # 设置优化器
    optimizer = optim.AdamW(policy_net.parameters(), lr=LR, amsgrad=True)
    memory = ReplayMemory(10000)

    steps_done = 0
    episode_durations = []

    # 设置训练次数
    if torch.cuda.is_available() or torch.backends.mps.is_available():
        num_episodes = EPS_DECAY
    else:
        num_episodes = 50

    # 开始训练
    for i_episode in range(num_episodes):
        # 初始化环境并获取其状态
        state, info = reset_policy()
        state = torch.tensor(state, dtype=torch.float32, device=device).unsqueeze(0)
        for t in count():
            # 选择动作
            action = select_action(state)
            # 执行动作并获取反馈
            observation, reward, terminated, truncated, _ = env.step(action.item())
            reward = torch.tensor([reward], device=device)
            done = terminated or truncated

            # 如果回合结束，下一个状态为None
            if terminated:
                next_state = None
            else:
                next_state = torch.tensor(observation, dtype=torch.float32, device=device).unsqueeze(0)

            # 将转换存储在记忆中
            memory.push(state, action, next_state, reward)

            # 移动到下一个状态
            state = next_state

            # 执行一步优化（在策略网络上）
            optimize_model()

            # 软更新目标网络的权重
            target_net_state_dict = target_net.state_dict()
            policy_net_state_dict = policy_net.state_dict()
            for key in policy_net_state_dict:
                target_net_state_dict[key] = policy_net_state_dict[key] * TAU + target_net_state_dict[key] * (1 - TAU)
            target_net.load_state_dict(target_net_state_dict)

            # 如果回合结束，记录持续时间并更新图像
            if done:
                episode_durations.append(t + 1)
                plot_durations(update_interval=10)
                break

    torch.save(target_net.state_dict(), 'models/sim_randon_reset.pth')
    print('Complete')
    # 显示结果
    plot_durations(show_result=True)
    plt.ioff()
    plt.show()
