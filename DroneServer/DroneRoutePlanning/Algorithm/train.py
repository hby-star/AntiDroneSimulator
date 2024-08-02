import json
import numpy as np
import torch
import torch.nn as nn
import torch.optim as optim
import random
import matplotlib.pyplot as plt

from collections import deque

from DroneRoutePlanning.Algorithm.DQN import DQN
from collections import namedtuple

Transition = namedtuple('Transition', ('state', 'action', 'reward', 'next_state', 'done'))


# 读取训练数据
def load_training_data(filename):
    with open(filename, 'r') as f:
        data = json.load(f)
    return data


# 解析训练数据
def parse_training_data(data):
    states = []
    actions = []
    rewards = []
    next_states = []
    dones = []
    for entry in data:
        state = [
            entry['DronePosition']['x'], entry['DronePosition']['y'], entry['DronePosition']['z'],
            entry['PlayerPositionInCamera']['x'], entry['PlayerPositionInCamera']['y'],
            entry['PlayerPositionInCamera']['z'], entry['PlayerPositionInCamera']['w'],
            entry['ObstalePosition']['x'], entry['ObstalePosition']['y'], entry['ObstalePosition']['z'],
        ]
        next_state = [
            entry['NextDronePosition']['x'], entry['NextDronePosition']['y'], entry['NextDronePosition']['z'],
            entry['PlayerPositionInCamera']['x'], entry['PlayerPositionInCamera']['y'],
            entry['PlayerPositionInCamera']['z'], entry['PlayerPositionInCamera']['w'],
            entry['ObstalePosition']['x'], entry['ObstalePosition']['y'], entry['ObstalePosition']['z'],
        ]
        action = entry['Direction']
        reward = entry['Reward']
        done = entry['Done']

        states.append(state)
        actions.append(action)
        rewards.append(reward)
        next_states.append(next_state)
        dones.append(done)
    return np.array(states), np.array(actions), np.array(rewards), np.array(next_states), np.array(dones)


def optimize_model():
    if len(memory) < batch_size:
        return
    transitions = random.sample(memory, batch_size)
    batch = Transition(*zip(*transitions))

    state_batch = torch.tensor(np.array(batch.state), dtype=torch.float32)  # Convert to float
    action_batch = torch.tensor(batch.action)
    reward_batch = torch.tensor(batch.reward).float()  # Convert to float
    next_state_batch = torch.tensor(np.array(batch.next_state), dtype=torch.float32)  # Convert to float
    done_batch = torch.tensor(np.array(batch.done, dtype=bool))

    state_action_values = model(state_batch).gather(1, action_batch.unsqueeze(1))
    next_state_values = torch.zeros(batch_size)
    next_state_values[~done_batch] = target_model(next_state_batch[~done_batch]).max(1)[0].detach()
    expected_state_action_values = (next_state_values * gamma) + reward_batch

    loss = criterion(state_action_values, expected_state_action_values.unsqueeze(1))
    optimizer.zero_grad()
    loss.backward()
    optimizer.step()
    losses.append(loss.item())


# 加载和解析训练数据
data = load_training_data('data/training_data.json')
states, actions, rewards, next_states, dones = parse_training_data(data)

# 初始化DQN模型
state_size = len(states[0])
action_size = len(actions[0])
model = DQN(state_size, action_size)

target_model = DQN(state_size, action_size)
target_model.load_state_dict(model.state_dict())

# 训练DQN模型
batch_size = 32
gamma = 0.99
learning_rate = 0.001
target_update = 10

optimizer = optim.Adam(model.parameters(), lr=learning_rate)
criterion = nn.MSELoss()

num_episodes = 50
memory = deque(maxlen=1000)

losses = []
average_losses = []

for episode in range(num_episodes):
    state = states[episode]
    for t in range(500):
        action = model(torch.tensor(state).float()).max(0)[1].item()
        next_state = next_states[episode]
        reward = rewards[episode]
        done = dones[episode]

        memory.append((state, action, reward, next_state, done))
        state = next_state

        optimize_model()

    # 打印训练进度
    average_loss = sum(losses) / len(losses) * 100
    print(f'Episode: {episode + 1}/{num_episodes}, Loss: {average_loss}')
    average_losses.append(average_loss)
    losses = []

    if episode % target_update == 0:
        target_model.load_state_dict(model.state_dict())

# 在训练结束后，绘制损失和准确率
plt.figure(figsize=(12, 4))
plt.subplot(1, 2, 1)
plt.plot(average_losses, label='Loss')
plt.legend()
plt.show()

# 保存模型参数
torch.save(model.state_dict(), 'model/model.pth')
