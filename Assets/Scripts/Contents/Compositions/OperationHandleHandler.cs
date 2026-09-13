using Cysharp.Threading.Tasks;
using System;
using VContainer;

namespace Commar.CubicLand.Compositions
{
    public class OperationHandleHandler
    {
        private IOperationHandle[] _operationHandles;

        public event Action<OperationResult> OnCompleted;

        [Inject]
        public void Initialize(IOperationHandle[] operationHandles)
        {
            _operationHandles = operationHandles;
        }

        public async UniTask WaitOperationHandlesAsync()
        {
            OperationResult result = OperationResult.GetSuccessResult();

            while (true)
            {
                bool isCompleted = true;

                foreach (IOperationHandle operationHandle in _operationHandles)
                {
                    if (operationHandle == null)
                    {
                        result = OperationResult.GetFailedResult("Null Operation Handle Included");
                        isCompleted = true;
                        break;
                    }

                    if (!operationHandle.IsCompleted)
                    {
                        isCompleted = false;
                        continue;
                    }

                    if (!operationHandle.Result.IsSuccess)
                    {
                        result = operationHandle.Result;
                        isCompleted = true;
                        break;
                    }
                }

                if (isCompleted)
                    break;

                await UniTask.Yield();
            }

            OnCompleted?.Invoke(result);
        }
    }
}