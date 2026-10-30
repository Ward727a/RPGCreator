// RPG Creator - Open-source RPG Engine.
// (c) 2026 Ward
// 
// This file is part of RPG Creator and is distributed under the Apache 2.0 License.
// You are free to use, modify, and distribute this file under the terms of the Apache 2.0 License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence Apache 2.0.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence Apache 2.0.
// Voir LICENSE pour plus de détails.
// 
// Contact:
// => Mail: Ward727a@gmail.com
//    Please use this object: "RPG Creator [YourObject]"
// => Discord: ward727
// 
// For urgent inquiries, sending both an email and a message on Discord is highly recommended for a quicker response.

using Microsoft.Extensions.DependencyInjection;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.PipelineManager;
using RPGCreator.Shared.Attributes;

namespace RPGCreator.EngineLib.PipelineManager;

[EngineService(typeof(IPipelineManager))]
public class PipelineManager(IServiceProvider serviceProvider) : IPipelineManager
{
    private readonly ScopedLogger Logger = SDK.Common.Logging.Logger.ForContext<PipelineManager>();
    
    private readonly Dictionary<Type, object> _sortedSteps = new();
    
    /// <summary>
    /// Pipeline step list.<br/>
    /// Type1 => Type of context<br/>
    /// List&lt;Type&gt; => List of pipeline steps
    /// </summary>
    private readonly Dictionary<Type, List<Type>> _pipelineStepContexts = new();

    /// <summary>
    /// Registers a pipeline step to be executed within the context of a specific type.
    /// </summary>
    /// <param name="contextType">
    /// The type that represents the context for which the pipeline step is being registered.
    /// </param>
    /// <param name="pipelineStepType">
    /// The type that represents the pipeline step to be registered.
    /// </param>
    public void RegisterPipelineStep(Type contextType, Type pipelineStepType)
    {
        if (_pipelineStepContexts.TryGetValue(contextType, out var listStep))
        {
            if(!listStep.Contains(pipelineStepType))
            {
                listStep.Add(pipelineStepType);
                Logger.Debug("Pipeline step {type} registered for context {contextType}", args: [pipelineStepType, contextType.Name]);
            }
            else
            {
                Logger.Error("Pipeline step {type} already registered for context {contextType}", args: [pipelineStepType, contextType.Name]);
            }
        }
        else
        {
            _pipelineStepContexts[contextType] = [pipelineStepType];
            Logger.Debug("Pipeline step {type} registered for context {contextType}", args: [pipelineStepType, contextType.Name]);
        }
    }

    public async ValueTask RunAsync<TContext>(TContext context)
    {
        var contextType = typeof(TContext);

        if (!_sortedSteps.TryGetValue(contextType, out var stepsObject))
        {
            if (!_pipelineStepContexts.TryGetValue(contextType, out var pipelineTypes))
                return;

            var steps = new List<IPipelineStep<TContext>>();
            
            foreach (var type in pipelineTypes)
            {
                var instance = (IPipelineStep<TContext>)ActivatorUtilities.CreateInstance(serviceProvider, type);
                steps.Add(instance);
            }

            var sortedList = TopologicalSort(steps);
            
            _sortedSteps[contextType] = sortedList;
            stepsObject = sortedList;
        }

        var sortedSteps = (List<IPipelineStep<TContext>>)stepsObject;

        foreach (var step in sortedSteps)
        {
            await step.RunAsync(context);
        }
    }

    private static List<IPipelineStep<TContext>> TopologicalSort<TContext>(IEnumerable<IPipelineStep<TContext>> steps)
    {
        var graph = new Dictionary<Type, StepInfo>();

        foreach (var step in steps)
        {
            var currentStepInfo = GetOrCreateInfo(step.GetType());
            currentStepInfo.Step = step;

            foreach (var runAfterStep in step.RunAfter)
            {
                var parentStepInfo = GetOrCreateInfo(runAfterStep);
                parentStepInfo.Childrens.Add(currentStepInfo);
                currentStepInfo.ParentsCount++;
            }

            foreach (var runBeforeStep in step.RunBefore)
            {
                var childStepInfo = GetOrCreateInfo(runBeforeStep);
                currentStepInfo.Childrens.Add(childStepInfo);
                childStepInfo.ParentsCount++;
            }
        }

        var toProcess = new Queue<StepInfo>();

        foreach (var stepInfo in graph.Values)
        {
            if(stepInfo.ParentsCount == 0)
                toProcess.Enqueue(stepInfo);
        }

        List<IPipelineStep<TContext>> sortedList = [];

        while (toProcess.Count > 0)
        {
            var currentStepInfo = toProcess.Dequeue();

            if (currentStepInfo.Step is not IPipelineStep<TContext> instance)
            {
                throw new InvalidOperationException($"Failure of pipeline: Module '{currentStepInfo.StepType.Name}' is required by a dependency but was not provided.");
            }
        
            sortedList.Add(instance);

            foreach (var childStepInfo in currentStepInfo.Childrens)
            {
                childStepInfo.ParentsCount--;

                if (childStepInfo.ParentsCount == 0)
                {
                    toProcess.Enqueue(childStepInfo);
                }
            }
        }
        
        if(sortedList.Count != graph.Count)
            throw new InvalidOperationException("Graph has circular dependencies");

        return sortedList;

        StepInfo GetOrCreateInfo(Type stepType)
        {
            if(graph.TryGetValue(stepType, out var stepInfo))
            {
                return stepInfo;
            }
            
            stepInfo = new StepInfo
            {
                StepType = stepType,
                Childrens = new List<StepInfo>(),
                
            };
            graph[stepType] = stepInfo;
            return stepInfo;
        }
    }
    
    private class StepInfo
    {
        public Type StepType { get; init; }
        public object Step { get; set; }
        public List<StepInfo> Childrens { get; init; }
        public int ParentsCount { get; set; }
    }
}