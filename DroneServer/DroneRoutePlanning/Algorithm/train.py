import numpy as np

from DroneRoutePlanning.Algorithm.DQNAgent import DQNAgent
from DroneRoutePlanning.Algorithm.DroneEnvironment import DroneEnvironment

env = DroneEnvironment(drone_position=np.array([0, 0, 0]), person_position_in_camera=np.array([0, 0, 0, 0]),
                       obstacle_positions=np.array([1, 1, 1]), screen_size=np.array([100, 100]),
                       detect_obstacle_distance=10)
agent = DQNAgent(state_size=env.state_size, action_size=env.action_size)
episodes = 1000
batch_size = 32

for e in range(episodes):
    state = env
    total_reward = 0
    for time in range(500):
        action = agent.act(state)
        next_state, reward, done = env.step(action)
        agent.remember(state, action, reward, next_state, done)
        state = next_state
        total_reward += reward
        if done:
            agent.update_target_model()
            print(f"Episode {e}/{episodes} finished with total reward: {total_reward}")
            break
    if len(agent.memory) > batch_size:
        agent.replay(batch_size)
