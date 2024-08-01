import numpy as np
from django.http import JsonResponse
from drf_yasg import openapi
from drf_yasg.utils import swagger_auto_schema
from rest_framework.decorators import api_view

from DroneRoutePlanning.Algorithm.DQN import predict_action
from DroneRoutePlanning.Algorithm.DroneEnvironment import drone_move_directions


@swagger_auto_schema(
    operation_summary="Drone Route Planning",
    tags=['Drone Route Planning'],
    methods=['POST'],
    request_body=openapi.Schema(
        type=openapi.TYPE_OBJECT,
        properties={
            'input': openapi.Schema(type=openapi.TYPE_STRING, description='Input for the route planning model')
        }
    ),
    responses={
        200: openapi.Schema(
            type=openapi.TYPE_OBJECT,
            properties={
                'output': openapi.Schema(type=openapi.TYPE_STRING, description='Output of the route planning model')
            }
        ),
        400: openapi.Schema(
            type=openapi.TYPE_OBJECT,
            properties={
                'error': openapi.Schema(type=openapi.TYPE_STRING, description='Error message')
            }
        ),
        500: openapi.Schema(
            type=openapi.TYPE_OBJECT,
            properties={
                'error': openapi.Schema(type=openapi.TYPE_STRING, description='Internal server error')
            }
        )}
)
@api_view(['POST'])
def drone_route_planning(request):
    try:
        # 解析请求中的无人机状态
        data = request.json()
        drone_position = np.array(data['DronePosition'])
        drone_velocity = np.array(data['DroneVelocity'])
        player_position_in_camera = np.array(data['PlayerPositionInCamera'])
        obstacle_info = np.array(data['ObstacleInfo']).flatten()

        # 组合成状态向量
        state = np.concatenate([drone_position, drone_velocity, player_position_in_camera, obstacle_info])

        # 预测动作
        action = predict_action(state)

        # 获取对应的移动方向
        direction = drone_move_directions[action]

        return JsonResponse({direction.tolist()})
    except Exception as e:
        return JsonResponse({'error': str(e)})
