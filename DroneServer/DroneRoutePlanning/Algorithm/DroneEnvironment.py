import math

import numpy as np

drone_move_directions = np.array([
    [1, 0, 0], [-1, 0, 0], [0, 1, 0], [0, -1, 0], [0, 0, 1], [0, 0, -1],
    [1, 1, 1], [-1, -1, -1], [1, 1, -1], [-1, -1, 1], [1, -1, 1], [-1, 1, -1],
    [1, -1, -1], [-1, 1, 1]
])


class DroneEnvironment:
    def __init__(self, drone_position, person_position_in_camera, obstacle_positions, screen_size,
                 detect_obstacle_distance):
        self.state_size = 10  # 状态空间大小 3+4+3
        self.action_size = 14  # 动作空间大小 14

        self.drone_position = drone_position  # 无人机位置
        self.person_position_in_camera = person_position_in_camera  # 人在相机中的位置
        self.obstacle_position = obstacle_positions  # 障碍物位置
        self.screen_size = screen_size  # 屏幕大小
        self.detect_obstacle_distance = detect_obstacle_distance  # 检测障碍物的距离

        self.state = np.concatenate(
            [self.drone_position, self.person_position_in_camera, self.obstacle_position])

    def step(self, action):
        move_direction = drone_move_directions[action]

        # 模拟无人机移动到新位置
        self.drone_position += move_direction

        # 模拟人在相机中位置的变化
        scale = np.linalg.norm(np.array(self.screen_size / 2)) / self.detect_obstacle_distance
        self.person_position_in_camera[0] -= move_direction[0] * scale
        self.person_position_in_camera[2] -= move_direction[0] * scale
        self.person_position_in_camera[1] -= move_direction[1] * scale
        self.person_position_in_camera[3] -= move_direction[1] * scale

        # 模拟障碍物位置的变化
        self.obstacle_position -= move_direction

        # 初始化done
        done = False

        # 计算追踪奖励，越靠近屏幕中心奖励越高，在屏幕之外则丢失目标
        screen_center = self.screen_size / 2
        person_center = [(self.person_position_in_camera[0] + self.person_position_in_camera[2]) / 2,
                         (self.person_position_in_camera[1] + self.person_position_in_camera[3]) / 2]
        distance = np.linalg.norm(np.array(person_center) - np.array(screen_center))
        distance /= np.linalg.norm(screen_center)
        if distance > np.linalg.norm(screen_center):
            done = True
            reward_tracking = -10
        else:
            reward_tracking = 1 / (1 + distance)

        # 计算障碍物奖励，与障碍物距离越近奖励越低
        distance = np.linalg.norm(self.drone_position - self.obstacle_position)
        distance /= self.detect_obstacle_distance
        if distance < 1:
            done = True
            reward_obstacle = -10
        else:
            reward_obstacle = 1 / (1 + distance)

        # 追踪奖励和障碍物奖励的加权和作为总奖励
        reward = reward_tracking + reward_obstacle

        state = np.concatenate(
            [self.drone_position, self.person_position_in_camera, self.obstacle_position])

        return state, reward, done
