using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace History
{
    public class HistoryUI : MonoBehaviour
    {
        const float W = 1080, H = 1920, PAD = 40, CW = 1000;
        static readonly Color BG = Cv(.96f), PANEL = Cv(1f), CARD = Cv(.97f);
        static readonly Color BORDER = Cv(.85f), DIV = Cv(.93f);
        static readonly Color BP = Cv(.2f), BS = Cv(.94f), BD = Cv(.88f);
        static readonly Color T1 = Cv(.13f), T2 = Cv(.53f), T3 = Cv(.73f);
        static Color Cv(float v) => new Color(v, v, v);

        static Font _f;
        static Font F => _f != null ? _f : (_f = Font.CreateDynamicFontFromOSFont("Arial", 14));

        HistoryGameManager gm;
        GameObject[] panels = new GameObject[10];

        // Global timer overlay
        GameObject timerOverlay;
        Text globalTimer;

        // Menu settings
        Text menuTimeTxt;
        int menuTimeIdx = 1; // index into TimeOptions, default 60s

        // 03 Examine
        Text eTimer, eCounter, ePlaced, eArtName, eZoneDesc;
        Text[] eNoteVal = new Text[5];
        Button[] eToolBtn = new Button[4];
        Image[] eToolBg = new Image[4];
        Transform eZoneParent;

        // 03a Reference
        int refPage;
        List<(ArtifactInfo, int)> refData;
        Text refPageTxt;
        GameObject[] refCards = new GameObject[3];
        Text[] refNm = new Text[3], refTr = new Text[3], refHW = new Text[3], refMt = new Text[3];
        Image[] refPhoto = new Image[3];

        // 02 Briefing
        Text bName, bDesc, bStats;
        Text[] bToolNm = new Text[4], bToolDs = new Text[4];
        Text[] bHallNm = new Text[3], bHallDs = new Text[3];

        // 04 ChooseHall
        Text hHint;
        Text[] hNm = new Text[3], hDs = new Text[3], hCt = new Text[3];

        // 05 ChooseWall
        Text wTitleTxt, wHint;
        Text[] wNm = new Text[3], wDs = new Text[3], wCt = new Text[3];

        // 06 Confirm
        Text cArt, cHall, cWall;

        // 07 Placed
        Text pTitle, pInfo, pRemain, pTimeTxt;

        // 08 Results
        Transform rList;
        Text rTotal, rRank, rFact;

        void Start()
        {
            gm = HistoryGameManager.I;
            Build();
            gm.OnStateChanged += OnState;
            gm.OnTraitRevealed += (a, b) => RefreshNotebook();
            gm.OnZoneExamined += z => { if (eZoneDesc != null) eZoneDesc.text = z.name + ": " + z.description; RefreshNotebook(); };
            OnState(GameState.Menu);
            Debug.Log($"[HistoryUI] Screen: {Screen.width}x{Screen.height} dpi={Screen.dpi}");
        }

        void Update()
        {
            if (gm == null) return;
            if (gm.TimerRunning)
            {
                int m = (int)(gm.TimeRemaining / 60);
                int s = (int)(gm.TimeRemaining % 60);
                string timeStr = m + ":" + s.ToString("D2");
                if (eTimer != null) eTimer.text = timeStr;
                if (globalTimer != null)
                {
                    globalTimer.text = timeStr;
                    globalTimer.color = gm.TimeRemaining < 10 ? new Color(0.8f, 0.2f, 0.2f) : T1;
                }
            }
            if (timerOverlay != null)
                timerOverlay.SetActive(gm.TimerRunning && gm.State != GameState.Examine && gm.State != GameState.Menu && gm.State != GameState.Briefing && gm.State != GameState.Results && gm.State != GameState.TimeUp);
        }

        // ============ BUILD ============

        void Build()
        {
            var cgo = new GameObject("HistoryCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var cv = cgo.GetComponent<Canvas>();
            cv.renderMode = RenderMode.ScreenSpaceOverlay;
            cv.sortingOrder = 100;
            var sc = cgo.GetComponent<CanvasScaler>();
            sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            sc.referenceResolution = new Vector2(W, H);
            sc.matchWidthOrHeight = 0f;
            sc.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

            if (FindFirstObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            var r = cgo.transform;
            Build01(r); Build02(r); Build03(r); Build03a(r);
            Build04(r); Build05(r); Build06(r); Build07(r);
            Build08(r); Build08a(r);

            // Global floating timer (visible on Reference, ChooseHall, ChooseWall, Confirm, Placed)
            timerOverlay = new GameObject("TimerOverlay", typeof(RectTransform));
            timerOverlay.transform.SetParent(r, false);
            var tort = timerOverlay.GetComponent<RectTransform>();
            tort.anchorMin = new Vector2(1, 1); tort.anchorMax = new Vector2(1, 1);
            tort.pivot = new Vector2(1, 1);
            tort.anchoredPosition = new Vector2(-PAD, -90);
            tort.sizeDelta = new Vector2(200, 60);
            globalTimer = Txt(timerOverlay.transform, "0:00", 0, 0, 200, 60, 34, T1, TextAnchor.MiddleRight);
            timerOverlay.SetActive(false);
        }

        // --- 01 MENU ---
        void Build01(Transform r)
        {
            var p = Pan("Menu", r); panels[0] = p; var t = p.transform;
            Txt(t, "ПОД", 0, 380, W, 100, 72, T1, TextAnchor.MiddleCenter);
            Txt(t, "ЗЕМЛЁЙ", 0, 470, W, 100, 72, T1, TextAnchor.MiddleCenter);
            Img(t, W / 2 - 200, 575, 400, 2, BORDER);
            Txt(t, "музейный хранитель", 0, 585, W, 50, 26, T3, TextAnchor.MiddleCenter);
            float bx = (W - 700) / 2;
            Btn(t, "Новая партия", bx, 720, 700, 100, 32, true, () => { Debug.Log("[UI] CLICK: Новая партия"); gm.StartNewRound(); });
            Btn(t, "Продолжить", bx, 850, 700, 100, 32, false, null).interactable = false;
            Btn(t, "Галерея", bx, 980, 700, 100, 32, false, null).interactable = false;
            Btn(t, "Настройки", bx, 1110, 700, 100, 32, false, null).interactable = false;

            // Time setting
            Txt(t, "Время раунда:", 0, 1300, W, 40, 24, T3, TextAnchor.MiddleCenter);
            Btn(t, "<", bx, 1350, 100, 80, 32, false, () => { menuTimeIdx = Mathf.Max(0, menuTimeIdx - 1); UpdateTimeSetting(); });
            menuTimeTxt = Txt(t, "60 сек", bx + 100, 1350, 500, 80, 32, T1, TextAnchor.MiddleCenter);
            Btn(t, ">", bx + 600, 1350, 100, 80, 32, false, () => { menuTimeIdx = Mathf.Min(HistoryGameManager.TimeOptions.Length - 1, menuTimeIdx + 1); UpdateTimeSetting(); });
            UpdateTimeSetting();
        }

        // --- 02 BRIEFING ---
        void Build02(Transform r)
        {
            var p = Pan("Briefing", r); panels[1] = p; var t = p.transform;
            Btn(t, "< Меню", PAD, 80, 200, 88, 26, false, () => gm.ReturnToMenu());
            Txt(t, "Новая партия", 0, 80, W, 60, 30, T1, TextAnchor.MiddleCenter);
            Img(t, PAD, 170, CW, 360, CARD);
            bName = Txt(t, "", 0, 190, W, 50, 38, T1, TextAnchor.MiddleCenter);
            bDesc = Txt(t, "", 80, 260, CW - 80, 140, 24, T2, TextAnchor.UpperLeft);
            bStats = Txt(t, "", 80, 450, CW - 80, 40, 24, T3);
            Txt(t, "Ваши инструменты:", PAD, 560, CW, 40, 28, T1);
            for (int i = 0; i < 4; i++)
            {
                float tx = PAD + (i % 2) * 510;
                float ty = 610 + (i / 2) * 130;
                Img(t, tx, ty, 490, 110, PANEL);
                bToolNm[i] = Txt(t, "", tx + 20, ty + 15, 450, 40, 26, T1);
                bToolDs[i] = Txt(t, "", tx + 20, ty + 55, 450, 40, 22, T3);
            }
            Txt(t, "Залы музея:", PAD, 890, CW, 40, 28, T1);
            for (int i = 0; i < 3; i++)
            {
                float hx = PAD + i * 340;
                Img(t, hx, 940, 320, 120, CARD);
                bHallNm[i] = Txt(t, "", hx + 15, 952, 290, 35, 22, T1, TextAnchor.MiddleCenter);
                bHallDs[i] = Txt(t, "", hx + 15, 992, 290, 50, 18, T3, TextAnchor.MiddleCenter);
            }
            Txt(t, "В каждом зале 3 стены: оружие · быт · документы", PAD, 1080, CW, 40, 22, T3);
            Txt(t, "Справочник с аналогами доступен во время осмотра.", PAD, 1120, CW, 40, 22, T2);
            var startBtn = Btn(t, "НАЧАТЬ РАУНД", PAD, 1700, CW, 100, 34, true, () => gm.BeginExamination());
            PosBot(startBtn.gameObject, PAD, 60, CW, 100);
        }

        // --- 03 EXAMINE ---
        void Build03(Transform r)
        {
            var p = Pan("Examine", r); panels[2] = p; var t = p.transform;
            Img(t, 0, 80, W, 80, Cv(.95f));
            eTimer = Txt(t, "5:00", PAD, 85, 200, 70, 38, T1);
            eCounter = Txt(t, "Артефакт 1/4", 0, 85, W, 70, 26, T2, TextAnchor.MiddleCenter);
            ePlaced = Txt(t, "Размещено: 0", W - 380, 85, 340, 70, 24, T3, TextAnchor.MiddleRight);
            Img(t, PAD, 155, CW, 4, Cv(.9f));
            Img(t, PAD, 180, CW, 420, Cv(.95f));
            eArtName = Txt(t, "[Артефакт]", 0, 350, W, 40, 26, T3, TextAnchor.MiddleCenter);
            Txt(t, "нажмите на область — увидите детали", 0, 560, W, 30, 20, T3, TextAnchor.MiddleCenter);
            var zp = new GameObject("Zones", typeof(RectTransform));
            zp.transform.SetParent(t, false); Pos(zp, PAD, 180, CW, 420);
            eZoneParent = zp.transform;
            eZoneDesc = Txt(t, "", PAD + 10, 605, CW - 20, 60, 22, T2);
            Img(t, 0, 680, W, 2, DIV);
            Txt(t, "Инструменты:", PAD, 690, CW, 40, 26, T1);
            for (int i = 0; i < 4; i++)
            {
                float bx = PAD + i * 250;
                var btn = Btn(t, "", bx, 735, 230, 90, 20, false, null);
                eToolBtn[i] = btn;
                eToolBg[i] = btn.GetComponent<Image>();
                int idx = i;
                btn.onClick.AddListener(() => { if (gm.Config.tools.Count > idx) gm.ApplyTool(gm.Config.tools[idx].id); RefreshExamine(); });
            }
            Img(t, 0, 840, W, 2, DIV);
            Txt(t, "Блокнот:", PAD, 850, CW, 40, 26, T1);
            Img(t, PAD, 895, CW, 270, CARD);
            string[] labels = { "Материал:", "Размер:", "Вес:", "Год:", "Язык текста:" };
            for (int i = 0; i < 5; i++)
            {
                float ny = 905 + i * 52;
                Txt(t, labels[i], PAD + 20, ny, 350, 40, 24, T3);
                eNoteVal[i] = Txt(t, "\u2014", PAD + 380, ny, 580, 40, 24, T1);
                if (i < 4) Img(t, PAD + 20, ny + 42, CW - 40, 1, DIV);
            }
            Btn(t, "Справочник — похожие артефакты", PAD, 1195, CW, 88, 26, false, () => gm.OpenReference());
            Btn(t, "Пропустить", PAD, 1310, 480, 90, 26, false, () => gm.SkipArtifact());
            Btn(t, "Разместить \u2192", PAD + 520, 1310, 480, 90, 26, true, () => gm.StartPlacement());
        }

        // --- 03a REFERENCE ---
        void Build03a(Transform r)
        {
            var p = Pan("Reference", r); panels[3] = p; var t = p.transform;
            Btn(t, "< Осмотр", PAD, 80, 220, 88, 26, false, () => gm.CloseReference());
            Txt(t, "Справочник", 0, 80, W, 60, 30, T1, TextAnchor.MiddleCenter);
            Txt(t, "Похожие на ваш артефакт:", PAD, 165, CW, 40, 24, T3);
            for (int i = 0; i < 3; i++)
            {
                float cy = 220 + i * 370;
                var card = new GameObject("RefCard" + i, typeof(RectTransform), typeof(Image));
                card.transform.SetParent(t, false); Pos(card, PAD, cy, CW, 350);
                card.GetComponent<Image>().color = CARD; card.GetComponent<Image>().raycastTarget = false;
                refCards[i] = card;
                refPhoto[i] = Img(card.transform, 20, 20, 180, 180, Cv(.9f));
                Txt(card.transform, "[фото]", 20, 80, 180, 40, 20, T3, TextAnchor.MiddleCenter);
                refNm[i] = Txt(card.transform, "", 220, 15, 750, 40, 28, T1);
                refTr[i] = Txt(card.transform, "", 220, 60, 750, 100, 22, T2);
                Img(card.transform, 220, 180, 720, 1, DIV);
                refHW[i] = Txt(card.transform, "", 220, 190, 750, 60, 22, T2);
                refMt[i] = Txt(card.transform, "", 220, 290, 750, 35, 20, T3, TextAnchor.MiddleRight);
            }
            Btn(t, "<", PAD, 1350, 120, 88, 32, false, () => { if (refPage > 0) { refPage--; RefreshRef(); } });
            refPageTxt = Txt(t, "1/1", 0, 1355, W, 80, 24, T2, TextAnchor.MiddleCenter);
            Btn(t, ">", W - PAD - 120, 1350, 120, 88, 32, false, () => { refPage++; RefreshRef(); });
            var backRef = Btn(t, "< Вернуться к осмотру", PAD, 1700, CW, 86, 26, false, () => gm.CloseReference());
            PosBot(backRef.gameObject, PAD, 60, CW, 86);
        }

        // --- 04 CHOOSE HALL ---
        void Build04(Transform r)
        {
            var p = Pan("ChooseHall", r); panels[4] = p; var t = p.transform;
            Btn(t, "< Осмотр", PAD, 80, 220, 88, 26, false, () => gm.BackToExamine());
            Txt(t, "Разместить", 0, 80, W, 60, 28, T1, TextAnchor.MiddleCenter);
            Img(t, PAD, 170, CW, 90, Cv(.97f));
            hHint = Txt(t, "", PAD + 20, 178, CW - 40, 74, 24, T2);
            Txt(t, "Шаг 1 из 2 — выберите зал", 0, 280, W, 40, 24, T3, TextAnchor.MiddleCenter);
            for (int i = 0; i < 3; i++)
            {
                float cy = 340 + i * 210;
                Img(t, PAD, cy, CW, 190, PANEL);
                hNm[i] = Txt(t, "", PAD + 30, cy + 20, 900, 50, 30, T1);
                hDs[i] = Txt(t, "", PAD + 30, cy + 75, 900, 40, 24, T2);
                hCt[i] = Txt(t, "", PAD + 30, cy + 125, 900, 40, 20, T3);
                var ov = Img(t, PAD, cy, CW, 190, new Color(0, 0, 0, 0));
                ov.raycastTarget = true;
                var btn = ov.gameObject.AddComponent<Button>();
                int idx = i;
                btn.onClick.AddListener(() => { if (gm.Config.halls.Count > idx) gm.SelectHall(gm.Config.halls[idx].id); });
            }
        }

        // --- 05 CHOOSE WALL ---
        void Build05(Transform r)
        {
            var p = Pan("ChooseWall", r); panels[5] = p; var t = p.transform;
            Btn(t, "< Залы", PAD, 80, 200, 88, 26, false, () => gm.BackToHalls());
            wTitleTxt = Txt(t, "Зал", 0, 80, W, 60, 28, T1, TextAnchor.MiddleCenter);
            Img(t, PAD, 170, CW, 90, Cv(.97f));
            wHint = Txt(t, "", PAD + 20, 178, CW - 40, 74, 24, T2);
            Txt(t, "Шаг 2 из 2 — выберите стену", 0, 280, W, 40, 24, T3, TextAnchor.MiddleCenter);
            for (int i = 0; i < 3; i++)
            {
                float cy = 340 + i * 210;
                Img(t, PAD, cy, CW, 190, PANEL);
                wNm[i] = Txt(t, "", PAD + 30, cy + 20, 900, 50, 30, T1);
                wDs[i] = Txt(t, "", PAD + 30, cy + 75, 900, 40, 24, T2);
                wCt[i] = Txt(t, "", PAD + 30, cy + 125, 900, 40, 20, T3);
                var ov = Img(t, PAD, cy, CW, 190, new Color(0, 0, 0, 0));
                ov.raycastTarget = true;
                var btn = ov.gameObject.AddComponent<Button>();
                int idx = i;
                btn.onClick.AddListener(() => { if (gm.Config.walls.Count > idx) gm.SelectWall(gm.Config.walls[idx].id); });
            }
        }

        // --- 06 CONFIRM ---
        void Build06(Transform r)
        {
            var p = Pan("Confirm", r); panels[6] = p;
            p.GetComponent<Image>().color = new Color(0, 0, 0, 0.4f);
            var t = p.transform;
            float mx = (W - 800) / 2;
            Img(t, mx, 480, 800, 520, PANEL);
            Txt(t, "Подтвердите", 0, 510, W, 50, 36, T1, TextAnchor.MiddleCenter);
            Img(t, mx + 40, 570, 720, 2, DIV);
            Txt(t, "Артефакт:", mx + 50, 590, 700, 35, 24, T3);
            cArt = Txt(t, "", mx + 50, 625, 700, 40, 28, T1);
            Img(t, mx + 40, 680, 720, 2, DIV);
            Txt(t, "Размещение:", mx + 50, 695, 700, 35, 24, T3);
            cHall = Txt(t, "", mx + 50, 730, 700, 40, 28, T1);
            cWall = Txt(t, "", mx + 50, 770, 700, 40, 28, T1);
            Img(t, mx + 40, 825, 720, 2, DIV);
            Txt(t, "Переставить нельзя", 0, 840, W, 40, 22, T3, TextAnchor.MiddleCenter);
            Btn(t, "Отмена", mx + 40, 900, 330, 80, 28, false, () => gm.CancelPlacement());
            Btn(t, "Да", mx + 430, 900, 330, 80, 28, true, () => gm.ConfirmPlacement());
        }

        // --- 07 PLACED ---
        void Build07(Transform r)
        {
            var p = Pan("Placed", r); panels[7] = p; var t = p.transform;
            Txt(t, "\u2713", 0, 500, W, 120, 96, T3, TextAnchor.MiddleCenter);
            pTitle = Txt(t, "Размещён!", 0, 630, W, 60, 40, T1, TextAnchor.MiddleCenter);
            pInfo = Txt(t, "", 0, 710, W, 40, 26, T2, TextAnchor.MiddleCenter);
            Img(t, W / 2 - 200, 770, 400, 2, DIV);
            pRemain = Txt(t, "", 0, 800, W, 40, 28, T3, TextAnchor.MiddleCenter);
            pTimeTxt = Txt(t, "", 0, 850, W, 40, 28, T3, TextAnchor.MiddleCenter);
            Txt(t, "Достаём следующий артефакт...", 0, 950, W, 40, 24, T3, TextAnchor.MiddleCenter);
        }

        // --- 08 RESULTS ---
        void Build08(Transform r)
        {
            var p = Pan("Results", r); panels[8] = p; var t = p.transform;
            Txt(t, "Итоги раунда", 0, 80, W, 60, 40, T1, TextAnchor.MiddleCenter);
            Img(t, PAD, 150, CW, 2, DIV);
            Txt(t, "АРТЕФАКТ", PAD + 10, 165, 500, 40, 22, T3);
            Txt(t, "ОЧКИ", W - PAD - 110, 165, 100, 40, 22, T3, TextAnchor.MiddleRight);
            Img(t, PAD, 205, CW, 2, DIV);
            var lp = new GameObject("ResultsList", typeof(RectTransform));
            lp.transform.SetParent(t, false); Pos(lp, 0, 215, W, 900);
            rList = lp.transform;
            Img(t, PAD, 1150, CW, 2, DIV);
            Txt(t, "Итого:", PAD + 10, 1170, 300, 50, 36, T1);
            rTotal = Txt(t, "0", W - PAD - 200, 1170, 190, 50, 48, T1, TextAnchor.MiddleRight);
            Img(t, PAD, 1240, CW, 110, Cv(.95f));
            Txt(t, "Ранг:", 0, 1250, W, 40, 26, T2, TextAnchor.MiddleCenter);
            rRank = Txt(t, "", 0, 1290, W, 50, 36, T1, TextAnchor.MiddleCenter);
            Img(t, PAD, 1370, CW, 100, CARD);
            rFact = Txt(t, "", PAD + 20, 1380, CW - 40, 80, 22, T2);
            var menuBtn = Btn(t, "В меню", PAD, 1700, 480, 90, 28, false, () => gm.ReturnToMenu());
            PosBot(menuBtn.gameObject, PAD, 60, 480, 90);
            var againBtn = Btn(t, "Ещё раунд", PAD + 520, 1700, 480, 90, 28, true, () => gm.StartNewRound());
            PosBot(againBtn.gameObject, PAD + 520, 60, 480, 90);
        }

        // --- 08a TIME UP ---
        void Build08a(Transform r)
        {
            var p = Pan("TimeUp", r); panels[9] = p;
            p.GetComponent<Image>().color = new Color(0, 0, 0, 0.4f);
            var t = p.transform;
            Img(t, (W - 700) / 2, 520, 700, 440, PANEL);
            Txt(t, "\u23F1", 0, 560, W, 80, 64, T3, TextAnchor.MiddleCenter);
            Txt(t, "Время вышло!", 0, 650, W, 60, 40, T1, TextAnchor.MiddleCenter);
            Img(t, (W - 600) / 2, 730, 600, 2, DIV);
            Txt(t, "Текущий артефакт\nне был размещён.", 0, 750, W, 80, 26, T2, TextAnchor.MiddleCenter);
            Btn(t, "Результаты \u2192", (W - 500) / 2, 860, 500, 80, 30, true, () => gm.ShowResults());
        }

        void UpdateTimeSetting()
        {
            int sec = HistoryGameManager.TimeOptions[menuTimeIdx];
            gm.TimeOverrideSec = sec;
            if (sec >= 60)
                menuTimeTxt.text = (sec / 60) + " мин" + (sec % 60 > 0 ? " " + (sec % 60) + " сек" : "");
            else
                menuTimeTxt.text = sec + " сек";
        }

        // ============ STATE ============

        void OnState(GameState s)
        {
            for (int i = 0; i < panels.Length; i++)
                if (panels[i] != null) panels[i].SetActive(false);
            int idx;
            switch (s)
            {
                case GameState.Menu: idx = 0; break;
                case GameState.Briefing: idx = 1; RefreshBriefing(); break;
                case GameState.Examine: idx = 2; RefreshExamine(); break;
                case GameState.Reference: idx = 3; refPage = 0; RefreshRef(); break;
                case GameState.ChooseHall: idx = 4; RefreshHall(); break;
                case GameState.ChooseWall: idx = 5; RefreshWall(); break;
                case GameState.Confirm: idx = 6; RefreshConfirm(); break;
                case GameState.Placed: idx = 7; RefreshPlaced(); break;
                case GameState.Results: idx = 8; RefreshResults(); break;
                case GameState.TimeUp: idx = 9; break;
                default: idx = 0; break;
            }
            if (panels[idx] != null) panels[idx].SetActive(true);
            if (s == GameState.Placed) StartCoroutine(CoAutoNext());
        }

        IEnumerator CoAutoNext()
        {
            yield return new WaitForSeconds(2f);
            if (gm.State == GameState.Placed) gm.NextArtifact();
        }

        // ============ REFRESH ============

        void RefreshBriefing()
        {
            var rd = gm.CurrentRound; if (rd == null) return;
            bName.text = rd.name;
            bDesc.text = rd.description;
            string stars = new string('\u2605', rd.difficulty) + new string('\u2606', 3 - rd.difficulty);
            int timeSec = gm.GetRoundTimeSec();
            string timeLabel = timeSec >= 60 ? (timeSec / 60) + " мин" : timeSec + " сек";
            bStats.text = "Артефактов: " + rd.artifacts.Count + "  ·  Время: " + timeLabel + "  ·  " + stars;
            for (int i = 0; i < 4 && i < gm.Config.tools.Count; i++)
            {
                bToolNm[i].text = gm.Config.tools[i].name;
                bToolDs[i].text = "\u2192 " + gm.Config.tools[i].description;
            }
            for (int i = 0; i < 3 && i < gm.Config.halls.Count; i++)
            {
                bHallNm[i].text = "Зал " + (i + 1);
                bHallDs[i].text = gm.Config.halls[i].name;
            }
        }

        void RefreshExamine()
        {
            var a = gm.CurrentArtifact; if (a == null) return;
            eCounter.text = "Артефакт " + (gm.CurrentArtifactIndex + 1) + " / " + gm.ArtifactTotal;
            ePlaced.text = "Размещено: " + gm.PlacedCount;
            eArtName.text = "[" + a.name + "]";
            eZoneDesc.text = "";
            for (int i = 0; i < 4; i++)
            {
                if (i < gm.Config.tools.Count)
                {
                    var tool = gm.Config.tools[i];
                    bool used = gm.UsedTools.Contains(tool.id);
                    var txt = eToolBtn[i].GetComponentInChildren<Text>();
                    txt.text = used ? tool.name + "\n\u2713 " + TraitSet.TraitLabel(tool.reveals) : tool.name + "\n" + TraitSet.TraitLabel(tool.reveals);
                    eToolBg[i].color = used ? BD : BS;
                    eToolBtn[i].interactable = !used;
                }
            }
            foreach (Transform c in eZoneParent) Destroy(c.gameObject);
            for (int i = 0; i < a.zones.Count; i++)
            {
                var zone = a.zones[i];
                float zx = 80 + i * 380;
                var zbtn = Btn(eZoneParent, zone.name, zx, 140, 280, 80, 22, false, null);
                zbtn.GetComponent<Image>().color = gm.ExaminedZones.Contains(zone.id) ? BD : new Color(1, 1, 1, 0.9f);
                string zid = zone.id;
                zbtn.onClick.AddListener(() => { gm.ExamineZone(zid); RefreshExamine(); });
            }
            RefreshNotebook();
        }

        void RefreshNotebook()
        {
            string[] ids = { "material", "size", "weight", "year", "textLanguage" };
            var a = gm.CurrentArtifact; if (a == null) return;
            for (int i = 0; i < 5; i++)
            {
                bool rev = gm.RevealedTraits.ContainsKey(ids[i]) && gm.RevealedTraits[ids[i]];
                eNoteVal[i].text = rev ? a.traits.GetValue(ids[i]) : "\u2014";
                eNoteVal[i].color = rev ? T1 : T3;
            }
        }

        void RefreshRef()
        {
            refData = gm.CatalogDB.FindSimilar(gm.CurrentArtifact, gm.RevealedTraits);
            int tp = Mathf.Max(1, Mathf.CeilToInt(refData.Count / 3f));
            refPage = Mathf.Clamp(refPage, 0, tp - 1);
            refPageTxt.text = (refPage + 1) + " / " + tp;
            for (int i = 0; i < 3; i++)
            {
                int di = refPage * 3 + i;
                bool show = di < refData.Count;
                refCards[i].SetActive(show);
                if (show)
                {
                    var (entry, pct) = refData[di];
                    refNm[i].text = entry.name;
                    refTr[i].text = entry.traits.material + " · " + entry.traits.weight + "г · " + entry.traits.size + " · ~" + entry.traits.year
                        + (string.IsNullOrEmpty(entry.traits.textLanguage) ? "" : "\nНадпись: " + entry.traits.textLanguage);
                    var h = gm.GetHall(entry.correctHall); var w = gm.GetWall(entry.correctWall);
                    refHW[i].text = "Зал: " + (h != null ? h.name : "?") + "\nСтена: " + (w != null ? w.name : "?");
                    refMt[i].text = "Совпадение: " + pct + "%";
                }
            }
        }

        void RefreshHall()
        {
            hHint.text = "Артефакт " + (gm.CurrentArtifactIndex + 1) + ": " + gm.GetNotebookSummary();
            for (int i = 0; i < 3 && i < gm.Config.halls.Count; i++)
            {
                var hall = gm.Config.halls[i];
                hNm[i].text = "Зал " + (i + 1) + " · " + hall.name;
                hDs[i].text = hall.description;
                int cnt = gm.GetPlacedInHall(hall.id);
                hCt[i].text = cnt > 0 ? "Размещено: " + cnt : "Пусто";
            }
        }

        void RefreshWall()
        {
            var hall = gm.GetHall(gm.ChosenHall);
            wTitleTxt.text = hall != null ? hall.name : "Зал";
            wHint.text = "Артефакт " + (gm.CurrentArtifactIndex + 1) + ": " + gm.GetNotebookSummary();
            for (int i = 0; i < 3 && i < gm.Config.walls.Count; i++)
            {
                var wall = gm.Config.walls[i];
                wNm[i].text = wall.name;
                wDs[i].text = wall.description;
                int cnt = gm.GetPlacedOnWall(gm.ChosenHall, wall.id);
                wCt[i].text = cnt > 0 ? "На стене: " + cnt : "Пусто";
            }
        }

        void RefreshConfirm()
        {
            cArt.text = gm.GetNotebookSummary();
            var h = gm.GetHall(gm.ChosenHall); var w = gm.GetWall(gm.ChosenWall);
            cHall.text = h != null ? h.name : "";
            cWall.text = w != null ? w.name : "";
        }

        void RefreshPlaced()
        {
            var last = gm.Placements.Count > 0 ? gm.Placements[gm.Placements.Count - 1] : null;
            if (last == null) return;
            if (last.skipped)
            {
                pTitle.text = "Пропущен";
                pInfo.text = last.artifact.name;
            }
            else
            {
                pTitle.text = "Размещён!";
                var h = gm.GetHall(last.chosenHall); var w = gm.GetWall(last.chosenWall);
                pInfo.text = "Артефакт \u2192 " + (h != null ? h.name : "") + " · " + (w != null ? w.name : "");
            }
            pRemain.text = "Осталось в ящике: " + gm.RemainingCount;
            int m = (int)(gm.TimeRemaining / 60); int s = (int)(gm.TimeRemaining % 60);
            pTimeTxt.text = "Время: " + m + ":" + s.ToString("D2");
        }

        void RefreshResults()
        {
            foreach (Transform c in rList) Destroy(c.gameObject);
            float y = 0;
            foreach (var p in gm.Placements)
            {
                Color col = p.skipped ? T3 : T1;
                Txt(rList, p.artifact.name, PAD + 10, y, 600, 35, 26, col);
                string st = p.skipped ? "не размещён" : "зал " + (p.hallCorrect ? "\u2713" : "\u2717") + " стена " + (p.wallCorrect ? "\u2713" : "\u2717");
                Txt(rList, st, PAD + 10, y + 35, 600, 30, 20, p.skipped ? T3 : T2);
                Txt(rList, p.skipped ? "0" : "+" + p.points, W - PAD - 150, y, 140, 65, 32, col, TextAnchor.MiddleRight);
                Img(rList, PAD, y + 70, CW, 1, DIV);
                y += 80;
            }
            int total = gm.GetTotalScore();
            rTotal.text = total.ToString();
            rRank.text = gm.GetRank(total) + " " + gm.GetRankStars(total);
            int em = (int)(gm.RoundElapsed / 60); int es = (int)(gm.RoundElapsed % 60);
            rTotal.text = total + "  (" + em + ":" + es.ToString("D2") + ")";
            string fact = "";
            foreach (var pp in gm.Placements)
            {
                if (!pp.skipped && pp.hallCorrect && pp.wallCorrect && !string.IsNullOrEmpty(pp.artifact.funFact))
                { fact = pp.artifact.funFact; break; }
            }
            if (string.IsNullOrEmpty(fact) && gm.Placements.Count > 0) fact = gm.Placements[0].artifact.funFact;
            rFact.text = "Факт: " + fact;
        }

        // ============ HELPERS ============

        RectTransform PosBot(GameObject go, float x, float fromBottom, float w, float h)
        {
            var rt = go.GetComponent<RectTransform>();
            if (rt == null) rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(0, 0);
            rt.pivot = new Vector2(0, 0);
            rt.anchoredPosition = new Vector2(x, fromBottom);
            rt.sizeDelta = new Vector2(w, h);
            return rt;
        }

        RectTransform Pos(GameObject go, float x, float y, float w, float h)
        {
            var rt = go.GetComponent<RectTransform>();
            if (rt == null) rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(w, h);
            return rt;
        }

        GameObject Pan(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = BG;
            go.SetActive(false);
            return go;
        }

        Text Txt(Transform parent, string text, float x, float y, float w, float h,
            int sz = 28, Color? col = null, TextAnchor align = TextAnchor.MiddleLeft)
        {
            var go = new GameObject("t", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            Pos(go, x, y, w, h);
            var t = go.AddComponent<Text>();
            t.text = text; t.font = F; t.fontSize = sz;
            t.color = col ?? T1; t.alignment = align;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        Button Btn(Transform parent, string label, float x, float y, float w, float h,
            int sz = 28, bool pri = true, Action onClick = null)
        {
            var go = new GameObject("b", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            Pos(go, x, y, w, h);
            go.GetComponent<Image>().color = pri ? BP : BS;
            var txt = Txt(go.transform, label, 0, 0, w, h, sz, pri ? Color.white : T1, TextAnchor.MiddleCenter);
            var trt = txt.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
            var btn = go.GetComponent<Button>();
            if (onClick != null) btn.onClick.AddListener(() => onClick());
            return btn;
        }

        Image Img(Transform parent, float x, float y, float w, float h, Color col)
        {
            var go = new GameObject("i", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Pos(go, x, y, w, h);
            var img = go.GetComponent<Image>();
            img.color = col; img.raycastTarget = false;
            return img;
        }
    }
}
