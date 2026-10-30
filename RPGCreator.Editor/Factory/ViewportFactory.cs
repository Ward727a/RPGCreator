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

using Fluxor;
using MediatR;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.EditorLib.Mediator.Services;
using RPGCreator.RTP.Viewport;
using RPGCreator.SDK.Editor.Rendering;
using RPGCreator.SDK.Factories;
using RPGCreator.SDK.Services.EngineService;
using RPGCreator.SDK.Services.RuntimeService;
using RPGCreator.SDK.States;
using RPGCreator.SDK.Types;

namespace RPGCreator.RTP.Factory;

public class ViewportFactory
{
    private readonly IInputsService _inputsService;
    private readonly IGameSimulationService _gameSimulationService;
    private readonly IBaseSubscriberFactory _baseSubscriberFactory;
    private readonly Dispatcher _stateDispatcher;
    private readonly ICameraService _cameraService;
    private readonly IChunkService _chunkService;
    private readonly IMapService _mapService;
    private readonly IRenderService _renderService;

    public ViewportFactory(IInputsService inputsService,
        IGameSimulationService gameSimulationService, IBaseSubscriberFactory baseSubscriberFactory,
        Dispatcher stateDispatcher, ICameraService cameraService, IChunkService chunkService, IMapService mapService, IRenderService renderService)
    {
        _inputsService = inputsService;
        _gameSimulationService = gameSimulationService;
        _baseSubscriberFactory = baseSubscriberFactory;
        _stateDispatcher = stateDispatcher;
        _cameraService = cameraService;
        _chunkService = chunkService;
        _mapService = mapService;
        _renderService = renderService;
    }

    public MonogameViewport CreateMonogameViewport(RenderTarget2D target2D)
    {
        return new MonogameViewport(target2D, _inputsService, _gameSimulationService, _baseSubscriberFactory,
            _stateDispatcher, _cameraService, _chunkService, _mapService, _renderService);
    }

    public UiViewport CreateUiViewport(RenderTarget2D target2D)
    {
        return new UiViewport(target2D, _inputsService);
    }
}