import torch
import numpy as np
from DroneRoutePlanning.Algorithm.DroneEnvironment import DroneEnvironment
from DroneRoutePlanning.Algorithm.DQN import DQN

if __name__ == '__main__':
    # Initialize the environment
    env = DroneEnvironment(drone_position=[0, 0, 0],
                           person_position_in_camera=[200, 140, 220, 150],
                           obstacle_positions=[5, 5, 5],
                           screen_size=[552, 326],
                           detect_obstacle_distance=10,
                           max_steps=1000)

    reset_policy = env.random_reset

    # Load the trained model
    device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
    n_actions = env.action_space.n
    state, info = reset_policy()
    n_observations = len(state)
    model = DQN(n_observations, n_actions).to(device)

    model.load_state_dict(torch.load('models/sim_reset.pth', weights_only=True))
    # model.load_state_dict(torch.load('model/sim_randon_reset.pth', weights_only=True))
    model.eval()

    # Test the model
    num_episodes = 50
    total_rewards_model = []
    total_rewards_random = []
    for i_episode in range(num_episodes):
        # Testing the model
        state, info = reset_policy()
        state = torch.tensor(state, dtype=torch.float32, device=device).unsqueeze(0)
        total_reward_model = 0
        done = False
        while not done:
            with torch.no_grad():
                action = model(state).max(1).indices.view(1, 1)
            next_state, reward, terminated, truncated, _ = env.step(action.item())
            total_reward_model += reward
            done = terminated or truncated
            if not done:
                state = torch.tensor(next_state, dtype=torch.float32, device=device).unsqueeze(0)
        total_rewards_model.append(total_reward_model)

        # Testing random actions
        state, info = reset_policy()
        total_reward_random = 0
        done = False
        while not done:
            action = np.random.choice(n_actions)  # Select a random action
            next_state, reward, terminated, truncated, _ = env.step(action)
            total_reward_random += reward
            done = terminated or truncated
        total_rewards_random.append(total_reward_random)

        print(
            f"Episode {i_episode + 1}: Reward Model = {total_reward_model}, Reward Random = {total_reward_random}")

    print(
        f"Average reward with model: {np.mean(total_rewards_model)}, Average reward with random actions: {np.mean(total_rewards_random)}")
