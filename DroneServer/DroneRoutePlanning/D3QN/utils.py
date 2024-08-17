import os
import time

import matplotlib.pyplot as plt
import torch


def create_directory(path: str, sub_dirs: list):
    for sub_dir in sub_dirs:
        if os.path.exists(path + sub_dir):
            print(path + sub_dir + 'is already exist!')
        else:
            os.makedirs(path + sub_dir, exist_ok=True)
            print(path + sub_dir + 'create successfully!')


def plot_learning_curve(episodes, records, title, ylabel, figure_file):
    plt.figure()
    plt.plot(episodes, records, linestyle='-', color='r')
    plt.title(title)
    plt.xlabel('episode')
    plt.ylabel(ylabel)

    plt.show()
    plt.savefig(figure_file)

def plot_durations(episode_reward_list, show_result=False, update_interval=100):
    try:
        # 每隔一定的间隔或者训练结束时更新图像
        if len(episode_reward_list) % update_interval != 0 and not show_result:
            return

        reward_t = torch.tensor(episode_reward_list, dtype=torch.float)

        if show_result:
            plt.title('Result')
        else:
            plt.title('Training...')
        plt.xlabel('Episode')
        plt.ylabel('Reward')
        plt.plot(episode_reward_list)
        # 计算并绘制每20个episode的平均Reward
        if len(reward_t) >= 20:
            means = reward_t.unfold(0, 20, 1).mean(1).view(-1)
            means = torch.cat((torch.zeros(19), means))
            plt.plot(means)

        # Show the figure on the screen

        plt.show()
    except Exception as e:
        time.sleep(1)
        return