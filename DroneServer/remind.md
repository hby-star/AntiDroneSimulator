# Remind

## 1. If occur error like this:
```shell
Traceback (most recent call last):
  File "C:\Users\dell\Desktop\AntiDroneSimulator\AntiDroneSimulator\DroneServer\DroneObjectDetection\Algorithm\test.py", line 1, in <module>
    import torch
  File "C:\Users\dell\Desktop\AntiDroneSimulator\AntiDroneSimulator\DroneServer\.venv\Lib\site-packages\torch\__init__.py", line 148, in <module>
    raise err
OSError: [WinError 126] 找不到指定的模块。 Error loading "C:\Users\dell\Desktop\AntiDroneSimulator\AntiDroneSimulator\DroneServer\.venv\Lib\site-packages\torch\lib\fbgemm.dll" or one of its dependencies.
```
Ref to: https://github.com/pytorch/pytorch/issues/131662


## 2. pipreqs error
```shell
pipreqs ./ --encoding UTF8 --force --use-local --ignore .venv
```
Ref to: https://www.fuxi.info/archives/257