import argparse

import gym
import numpy as np
import torch

from D3QN import D3QN
from DroneRoutePlanning.DroneEnvironment.DroneEnvironment import DroneEnvironment
from utils import create_directory, plot_learning_curve, plot_durations

parser = argparse.ArgumentParser()
parser.add_argument('--max_episodes', type=int, default=500)
parser.add_argument('--ckpt_dir', type=str, default='./checkpoints/D3QN/')
parser.add_argument('--reward_path', type=str, default='./output_images/reward.png')
parser.add_argument('--epsilon_path', type=str, default='./output_images/epsilon.png')

args = parser.parse_args()


def main():
    env = DroneEnvironment(person_position_in_camera=[200, 140, 220, 150],
                           obstacle_positions=[5, 5, 5],
                           screen_size=[552, 326],
                           detect_obstacle_distance=10,
                           max_steps=2000)
    agent = D3QN(alpha=0.0003, state_dim=env.observation_space.shape[0], action_dim=env.action_space.n,
                 fc1_dim=256, fc2_dim=256, ckpt_dir=args.ckpt_dir, gamma=0.99, tau=0.05, epsilon=1.0,
                 eps_end=0.05, eps_dec=5e-4, max_size=1000000, batch_size=256)
    create_directory(args.ckpt_dir, sub_dirs=['Q_eval', 'Q_target'])
    total_rewards, avg_rewards, epsilon_history = [], [], []

    for episode in range(args.max_episodes):
        total_reward = 0
        done = False
        truncated = False
        observation, info = env.reset()
        while not done and not truncated:
            action = agent.choose_action(np.array(observation), isTrain=True)
            observation_, reward, done, truncated, info = env.step(action)
            agent.remember(observation, action, reward, np.array(observation_), done)
            agent.learn()
            total_reward += reward
            observation = observation_

        total_rewards.append(total_reward)
        avg_reward = np.mean(total_rewards[-100:])
        avg_rewards.append(avg_reward)
        epsilon_history.append(agent.epsilon)
        print('EP:{} Reward:{} Avg_reward:{} Epsilon:{}'.
              format(episode + 1, total_reward, avg_reward, agent.epsilon))
        plot_durations(total_rewards, False, update_interval=10)

        if (episode + 1) % 50 == 0:
            agent.save_models(episode + 1)

    episodes = [i + 1 for i in range(args.max_episodes)]
    plot_learning_curve(episodes, avg_rewards, title='Reward', ylabel='reward',
                        figure_file=args.reward_path)
    plot_learning_curve(episodes, epsilon_history, title='Epsilon', ylabel='epsilon',
                        figure_file=args.epsilon_path)


if __name__ == '__main__':
    main()
