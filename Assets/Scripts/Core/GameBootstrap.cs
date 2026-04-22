using IdleSoccerClubMVP.Services.Interfaces;
using IdleSoccerClubMVP.Services.Local;
using IdleSoccerClubMVP.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace IdleSoccerClubMVP.Core
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        private IProgressService progressService;
        private IEconomyService economyService;
        private IScoutService scoutService;
        private IConfigProvider configProvider;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateBootstrap()
        {
            GameObject bootstrapObject = new GameObject("IdleSoccerClubMVP_Bootstrap");
            DontDestroyOnLoad(bootstrapObject);
            bootstrapObject.AddComponent<GameBootstrap>();
        }

        private void Awake()
        {
            EnsureEventSystem();
            configProvider = new LocalConfigProvider();
            LocalGameSession session = new LocalGameSession(configProvider, new LocalSaveRepository());
            progressService = new LocalProgressService(session);
            economyService = new LocalEconomyService(session);
            scoutService = new LocalScoutService(session);
            progressService.Initialize();
            CreateUiRoot();
        }

        private void Update()
        {
            if (progressService != null)
            {
                progressService.Tick();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && progressService != null)
            {
                progressService.RecordShutdown();
            }
        }

        private void OnApplicationQuit()
        {
            if (progressService != null)
            {
                progressService.RecordShutdown();
            }
        }

        private void CreateUiRoot()
        {
            Canvas existingCanvas = FindObjectOfType<Canvas>();
            if (existingCanvas != null)
            {
                GameUiRoot existingRoot = existingCanvas.GetComponentInChildren<GameUiRoot>();
                if (existingRoot != null)
                {
                    existingRoot.Initialize(progressService, economyService, scoutService, configProvider);
                    return;
                }
            }

            GameObject canvasObject = new GameObject("GameCanvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1080f, 1920f);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 1f;
            canvasObject.AddComponent<GraphicRaycaster>();
            DontDestroyOnLoad(canvasObject);

            GameUiRoot uiRoot = canvasObject.AddComponent<GameUiRoot>();
            uiRoot.Initialize(progressService, economyService, scoutService, configProvider);
        }

        private static void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
            DontDestroyOnLoad(eventSystemObject);
        }
    }
}
