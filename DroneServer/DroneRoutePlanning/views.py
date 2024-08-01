from PIL import Image
from django.http import JsonResponse
from drf_yasg import openapi
from drf_yasg.utils import swagger_auto_schema
from rest_framework.decorators import api_view

from DroneRoutePlanning.models import RoutePlanningRequest


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
        )}
)
@api_view(['POST'])
def drone_route_planning(request):
    try:
        RoutePlanningRequest.objects.create(input=request.data['input'])
        return JsonResponse({'output': 'Route planning request received'})
    except Exception as e:
        return JsonResponse({'error': str(e)})
