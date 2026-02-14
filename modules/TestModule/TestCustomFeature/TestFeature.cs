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

using RPGCreator.SDK;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Systems;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Modules.Features.Entity;
using RPGCreator.SDK.Types;

namespace TestModule.TestCustomFeature;

/// <summary>
/// This is just a test feature.<br/>
/// It can also be used as a template for creating new features.<br/>
/// A feature is a modular component that can be added to entities to provide them with specific functionalities.<br/>
/// Each feature can have its own configuration properties, dependencies, and behavior defined through systems and components.<br/>
/// And each feature can be used multiple times on different entities, depending on the "MaxInstancesPerCharacter" property defined in the <see cref="EntityFeatureAttribute"/>.<br/>
/// <br/>
/// Each feature declared will be automatically discovered by the engine at startup, and will be available in the editor for use on entities.
/// </summary>
[EntityFeature]
public class TestFeature : BaseEntityFeature
{
    /// <summary>
    /// This feature depends on the MyVeryOwnTestDependencyFeature being present on the entity.<br/>
    /// This is just an example of how to declare dependencies between features, right now, no feature with the urn 'rpgc://entity_features/MyVeryOwnTestDependencyFeature' exists, so this feature will not be usable until such a feature is created.
    /// </summary>
    public override URN[] DependentFeatures { get; } = [new("rpgc://entity_features/MyVeryOwnTestDependencyFeature")];
    public override string FeatureName => "Test feature (DO NOT USE!!)";
    public override string FeatureDescription => "This is just a test feature. Please, do not use it in production (in fact, do not use it at all, it literally does nothing, and should not even be present by default).";
    
    // Important note about the FeatureUrn:
    //
    // You can use other module urn (FeatureUrnModule), but be aware that if you do that, the engine will give you a warning.
    // This is due to the fact that I still don't know if it should be allowed or not, for now it's working, but it may change in the future.
    //
    // If this change, and the "FeatureUrnModule" is obligatory, then I will make it so the engine will automatically change it to the correct one,
    // Just be aware that it could cause issues if you are using a different module urn.
    //
    public override URN FeatureUrn => FeatureUrnModule.ToUrnModule("rpgc").ToUrn("TestFeature");

    public override string FeatureIcon => EngineServices.ModulePathResolver.ResolveFilePath(new URN("Ward727", "module", "TestModule/Folder"), "feature_icon.png");

    /*
     *
     * Important for the category:
     * 
     * By default, the category is "Settings".
     * If you want to create subcategories, use slashes ("/") to separate them.
     * The order of categories is determined by the order you define them here.
     * For example, if you have 3 variables (A, B, C), with 2 categories (A & B: "Settings", C: "Settings/Advanced"),
     * and you do this:
     * int A;
     * int C;
     * int B;
     *
     * In the editor, the order will be:
     * > Settings
     * > A
     * > > Advanced
     * > > C
     * > B
     *
     * The "Advanced" subcategory will appear after "A" and before "B", as it's defined that way (A, then C, then B).
     * 
     */
    [EntityFeatureProperty("Test Integer", "This is just a test integer property.", MaxValue = 20, MinValue = 0)]
    public int TestInt
    {
        get => GetConfig(10);
        set => SetConfig(value);
    }
    [EntityFeatureProperty("Test Bool", "This is just a test Bool property.", Category = "My own category/with sub path")]
    public bool TestBool
    {
        get => GetConfig(false);
        set => SetConfig(value);
    }
    
    [EntityFeatureProperty("Test String", "This is just a test String property.", Category = "My own category/with sub path", IsShared = true)]
    public string TestString
    {
        get => GetShared("Hello world!");
        set => SetShared(value);
    }
    
    public override void OnSetup()
    {
        // Initialization logic here
        // This is only called once when the feature is loaded by the engine.
        // This is NOT called when the feature is added to an entity.
    }
    
    public override void OnWorldSetup(IEcsWorld world)
    {
        // World setup logic here
        // This is called once per ECS world, if any entity in the world has this feature.
        // PLEASE NOTE: All the config data here is the default config data (here TestInt WILL be equal to 10), NOT the entity-specific one!!!
        // If you need entity-specific data, use the OnInject method instead.
        Logger.Debug("TestInt: {testInt}", TestInt); // This will always print: "TestInt: 10"
        world.SystemManager.AddSystem(new TestFeatureSystem());
    }

    public override void OnInject(BufferedEntity entity, IEntityDefinition entityDefinition)
    {
        entity.AddComponent(new TestFeatureComponent
        {
            TestInt = TestInt
        });
    }

    public override void OnDestroy(BufferedEntity entity)
    {
        entity.RemoveComponent<TestFeatureComponent>();
    }

    public override void Dispose()
    {
        // Cleanup logic here
        // This is called when the feature is being unloaded by the engine.
    }
}

public struct TestFeatureComponent : IComponent
{
    public int TestInt;
}

public class TestFeatureSystem : ISystem
{
    public override int Priority => -10000;
    public override bool IsDrawingSystem => false;
    
    private IEcsWorld _world;
    
    public override void Initialize(IEcsWorld ecsWorld)
    {
        // Initialization logic here
        _world = ecsWorld;
    }

    // Note for this: the "Update" method is used for non-drawing systems (if IsDrawingSystem is false) AND drawing systems (if IsDrawingSystem is true).
    public override void Update(TimeSpan deltaTime)
    {
        // Update logic here
        var entityWithFeature = _world.ComponentManager.Query<TestFeatureComponent>();
        
        foreach (var entityId in entityWithFeature)
        {
            ref var featureComponent = ref _world.ComponentManager.GetComponent<TestFeatureComponent>(entityId);
            // Example logic: just print the TestInt value
            // Note: In a real system, avoid using logging in tight loops for performance reasons.
            Logger.Debug($"Entity {entityId} has TestInt value: {featureComponent.TestInt}");
        }
    }
}