﻿namespace PubDoomer.Engine.TaskInvokation.TaskDefinition;

public interface ITaskHandler
{
    ValueTask<bool> HandleAsync();
}