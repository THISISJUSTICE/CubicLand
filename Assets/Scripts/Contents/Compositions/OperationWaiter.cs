using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using VContainer;

namespace Commar.CubicLand.Compositions
{
    public class OperationWaiter
    {
        private IReadOnlyList<IOperationHandle> _operationHandles;

        public event Action<OperationResult> OnCompleted;

        [Inject]
        public void Initialize(IReadOnlyList<IOperationHandle> operationHandles)
        {
            _operationHandles = operationHandles;
        }

        public async UniTask WaitOperationHandlesAsync()
        {
            OperationResult result = OperationResult.GetSuccessResult();

            while (true)
            {
                bool isCompleted = true;

                for (int i = 0; i < _operationHandles.Count; i++)
                {
                    IOperationHandle operationHandle = _operationHandles[i];

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