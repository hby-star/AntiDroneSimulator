import math
import random

import gym
from gym import spaces
import numpy as np

drone_move_directions = np.array([
    [-2, 0, 4], [-1, 0, 4], [0, 0, 4], [1, 0, 4], [2, 0, 4],

    [-2, 2, 4], [-1, 2, 4], [0, 2, 4], [1, 2, 4], [2, 2, 4],

    [-2, -2, 4], [-1, -2, 4], [0, -2, 4], [1, -2, 4], [2, -2, 4]
], dtype=float)


class DroneEnvironment(gym.Env):
    def __init__(self, person_position_in_camera, obstacle_positions, screen_size,
                 detect_obstacle_distance, max_steps=1000):
        super(DroneEnvironment, self).__init__()

        self.state_size = 7  # 状态空间大小 4+3
        self.action_size = len(drone_move_directions)  # 动作空间大小 15
        self.max_steps = max_steps
        self.current_step = 0

        # 状态空间
        self.person_position_in_camera = np.array(person_position_in_camera, dtype=float)  # 人在相机中的位置
        self.obstacle_position = np.array(obstacle_positions, dtype=float)  # 障碍物位置

        self.screen_size = np.array(screen_size, dtype=float)  # 屏幕大小
        self.detect_obstacle_distance = detect_obstacle_distance  # 检测障碍物的距离

        self.observation_space = spaces.Box(low=-np.inf, high=np.inf, shape=(self.state_size,), dtype=np.float32)
        self.action_space = spaces.Discrete(self.action_size)

        self.state = self._get_state()

    def _get_state(self):
        return np.concatenate([self.person_position_in_camera, self.obstacle_position])

    def step(self, action):
        move_direction = drone_move_directions[action]
        move_direction /= np.linalg.norm(move_direction)
        self.current_step += 1

        # 模拟人在相机中位置的变化
        scale = 10
        self.person_position_in_camera[0] -= move_direction[0] * scale
        self.person_position_in_camera[2] -= move_direction[0] * scale
        self.person_position_in_camera[1] -= move_direction[1] * scale
        self.person_position_in_camera[3] -= move_direction[1] * scale

        # 模拟障碍物位置的变化
        self.obstacle_position = self.obstacle_position - move_direction

        # 初始化done
        done = False
        truncated = False

        # 计算追踪奖励，越靠近屏幕中心奖励越高，在屏幕之外则丢失目标
        screen_center = self.screen_size / 2
        person_center = [(self.person_position_in_camera[0] + self.person_position_in_camera[2]) / 2,
                         (self.person_position_in_camera[1] + self.person_position_in_camera[3]) / 2]
        distance = np.linalg.norm(np.array(person_center) - np.array(screen_center))
        if person_center[0] < 0 or person_center[0] > self.screen_size[0] or \
                person_center[1] < 0 or person_center[1] > self.screen_size[1]:
            done = True
            reward_tracking = -2
        else:
            distance /= np.linalg.norm(screen_center)
            reward_tracking = 1 / (1 + distance)

        # 计算障碍物奖励，与障碍物距离越近奖励越低
        distance = np.linalg.norm(self.obstacle_position)
        if distance < 1:
            done = True
            reward_obstacle = -2
        else:
            distance /= self.detect_obstacle_distance
            reward_obstacle = 1 / (1 + distance)

        # 追踪奖励和障碍物奖励的加权和作为总奖励
        reward = reward_tracking + reward_obstacle
        # reward = reward_tracking

        self.state = self._get_state()

        if self.current_step >= self.max_steps:
            truncated = True

        return self.state, reward, done, truncated, {}

    def reset(self):
        self.person_position_in_camera = np.array([215, 160, 225, 170])
        self.obstacle_position = np.array(self.reset_obstacle_position())
        self.current_step = 0
        self.screen_size = np.array([552, 326])
        self.detect_obstacle_distance = 10
        self.state = self._get_state()
        return self.state, {}

    def reset_obstacle_position(self):
        x = 5 if random.random() < 0.5 else -5
        y = 5 if random.random() < 0.5 else -5
        z = 5 if random.random() < 0.5 else -5
        return [x, y, z]

    def random_reset(self):
        self.person_position_in_camera = self.random_person_position_in_camera()
        self.obstacle_position = self.random_obstacle_position()
        self.current_step = 0
        self.screen_size = np.array([552, 326])
        self.detect_obstacle_distance = 10
        self.state = self._get_state()
        return self.state, {}

    def random_person_position_in_camera(self):
        x1 = random.randint(0, int(self.screen_size[0]))
        x2 = random.randint(x1, int(self.screen_size[0]))
        y1 = random.randint(0, int(self.screen_size[1]))
        y2 = random.randint(y1, int(self.screen_size[1]))
        return [x1, y1, x2, y2]

    def random_obstacle_position(self):
        x = random.randint(-5, 5)
        y = random.randint(-5, 5)
        z = random.randint(-5, 5)
        return [x, y, z]

# # 使用自定义环境
# env = DroneEnvironment(drone_position=[0, 0, 0],
#                        person_position_in_camera=[200, 140, 220, 150],
#                        obstacle_positions=[5, 5, 5],
#                        screen_size=[552, 326],
#                        detect_obstacle_distance=10)
#
# # 示例：使用环境
# state, info = env.reset()
# done = False
# while not done:
#     action = env.action_space.sample()  # 这里随机选择一个动作
#     state, reward, done, truncated, info = env.step(action)
#     print(f"State: {state}, Reward: {reward}, Done: {done}")
