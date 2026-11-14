using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPoolImplementation
{
     PoolCreator CreatePoolObject();

     void OnGet(PoolCreator obj);
     void OnRelease(PoolCreator obj);
     void OnDestroyPoolObject(PoolCreator obj);
}
