# AntiDroneSimulator

## 1. 项目简介

​	本项目使用Unity进行开发，在此项目中，玩家与无人机集群进行对抗。无人机集群使用人工蜂群算法（改）进行组织，玩家有多种枪械可供选择，每种枪械特点不同。支持自定义无人机集群，玩家等。支持双屏显示，单个屏幕下仅显示玩家视角，双屏时可额外显示无人机视角；接入[OpenAI whisper](https://openai.com/index/whisper/)模型进行语音识别控制队友的行动。

## 2. 项目进展

​	目前已开发出windows和Steam VR版，见 [Release](https://github.com/hby-star/AntiDroneSimulator/releases)。Steam VR版适配Pico Controller和KAT walk mini S。

## 3. 用户手册

​	见项目目录下 [UserManual.md](https://github.com/hby-star/AntiDroneSimulator/blob/vr_steam/UserManual.md) 。

## 4. 演示

​	见项目下[Video](https://github.com/hby-star/AntiDroneSimulator/tree/vr_steam/Video)文件夹

## 5. Thanks to:

* https://github.com/Macoron/whisper.unity A Unity3d bindings for [whisper.cpp](https://github.com/ggerganov/whisper.cpp). It provides high-performance inference of [OpenAI's Whisper](https://github.com/openai/whisper) automatic speech recognition (ASR) model running on your local machine.
* [Free Assets](https://github.com/hby-star/AntiDroneSimulator/tree/vr_steam/Assets/ExternalAssets ) from [Unity Asset Store](https://assetstore.unity.com/) .
