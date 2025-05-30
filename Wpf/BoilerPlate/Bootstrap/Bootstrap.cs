﻿using System;
using Unity;

namespace BoilerPlate.Bootstrap;

public class Bootstrap
{
    IUnityContainer _container = new UnityContainer();
    public void Initialize()
    {
        PrepareApplication();
    }

    private void PrepareApplication()
    {
        IoC.GetInstance = GetInstance;

        RegisterServices();
    }

    private void RegisterServices()
    {
    }

    public object GetInstance(Type type)
    {
        return _container.Resolve(type);
    }
}
