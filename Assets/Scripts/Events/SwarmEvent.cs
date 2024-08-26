public static class SwarmEvent
{
    // Detect Drone --> Swarm
    public const string DETECT_DRONE_FOUND_PLAYER = "DETECT_DRONE_FOUND_PLAYER";
    public const string DETECT_DRONE_ASK_FOR_NEW_HONEY = "DETECT_DRONE_ASK_FOR_NEW_HONEY";

    // Attack Drone --> Swarm
    public const string ATTACK_DRONE_PLAYER_DIED = "ATTACK_DRONE_PLAYER_DIED";

    // Swarm --> All Drones
    public const string SWARM_BACK_TO_HIVE = "SWARM_BACK_TO_HIVE";
}
