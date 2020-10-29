﻿using EuNet.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace EuNet.Unity
{
    public class NetViews : INetSerializable
    {
        private Dictionary<int, NetView> _viewMap;
        public Dictionary<int, NetView> Views => _viewMap;

        private HashSet<int> _generatedViewIds;

        private int _lastGenerateViewId = 0;
        private int _lastGenerateSceneViewId = 0;

        public int ViewCount => _viewMap.Count;
        public const int MaxGenerateViewIdPerSession = 1000;

        public NetViews()
        {
            _viewMap = new Dictionary<int, NetView>();
            _generatedViewIds = new HashSet<int>();
        }

        public void Clear()
        {
            _viewMap.Clear();
            _generatedViewIds.Clear();
        }

        public bool RegisterView(NetView view)
        {
            if (view.ViewId == 0)
            {
                Debug.LogError($"invalid viewId [{view.gameObject.name}]", view);
                return false;
            }

            if (_viewMap.ContainsKey(view.ViewId))
            {
                Debug.LogError($"already registered viewId [{view.ViewId}] origin [{_viewMap[view.ViewId].gameObject.name}] try [{view.gameObject.name}]",
                    _viewMap[view.ViewId]);
                return false;
            }

            _viewMap[view.ViewId] = view;

            return true;
        }

        public bool UnregisterView(NetView view)
        {
            if (view.ViewId == 0)
            {
                Debug.LogWarning($"invaild viewId [{view.gameObject.name}]", view);
                return false;
            }

            return _viewMap.Remove(view.ViewId);
        }

        public NetView Find(int viewId)
        {
            NetView view;
            _viewMap.TryGetValue(viewId, out view);
            return view;
        }

        internal int GenerateViewId(int sessionId)
        {
            if (sessionId == 0)
            {
                // Scene 오브젝트
                int newSubId = _lastGenerateSceneViewId;
                int newId = 0;

                for (int i = 1; i < MaxGenerateViewIdPerSession; i++)
                {
                    newSubId = (newSubId + 1) % MaxGenerateViewIdPerSession;
                    if (newSubId == 0)
                        continue;

                    newId = newSubId;

                    if (_generatedViewIds.Contains(newId) == false &&
                        _viewMap.ContainsKey(newId) == false)
                    {
                        _generatedViewIds.Add(newId);
                        _lastGenerateSceneViewId = newId;

                        //Debug.Log($"GenerateViewId {newId} HashMap Size {_generatedViewIds.Count}");
                        return newId;
                    }
                }

                throw new Exception($"GenerateViewId() failed. (sessionId {sessionId}) is out of SCENE viewIds. It seems all available are in use.");
            }
            else
            {
                // 세션이 소유하는 오브젝트
                int newSubId = _lastGenerateViewId;
                int newIdBase = sessionId * MaxGenerateViewIdPerSession;
                int newId = 0;

                for (int i = 1; i < MaxGenerateViewIdPerSession; i++)
                {
                    newSubId = (newSubId + 1) % MaxGenerateViewIdPerSession;
                    newId = newIdBase + newSubId;

                    if (_generatedViewIds.Contains(newId) == false &&
                        _viewMap.ContainsKey(newId) == false)
                    {
                        _generatedViewIds.Add(newId);
                        _lastGenerateViewId = newId;
                        return newId;
                    }
                }

                throw new Exception($"GenerateViewId() failed. SessionId {sessionId} is out of viewIds, as all viewIds are used.");
            }
        }

        public void RemoveViewId(int viewId)
        {
            _generatedViewIds.Remove(viewId);

            if (_viewMap.ContainsKey(viewId) == true)
            {
                // 자동으로 NetView 에서 View를 등록제거하고 ViewId를 제거요청해야하는데 순서가 바뀌었음.
                // 유저가 수동으로 삭제하지 않아야 함
                Debug.LogWarning($"RemoveViewId() should be called after the NetView was destroyed (GameObject.Destroy()). ViewId: {viewId} still found in: {_viewMap[viewId].gameObject.name}", _viewMap[viewId].gameObject);
                return;
            }

            //Debug.Log($"RemoveViewId {viewId} HashMap Size {_generatedViewIds.Count}");
        }

        public void Update(float elapsedTime)
        {
            foreach (var kvp in _viewMap)
            {
                kvp.Value.OnUpdate(elapsedTime);
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Write((int)_viewMap.Count);
            Debug.Log($"Request recovery view count : {_viewMap.Count}");

            foreach (var kvp in _viewMap)
            {
                var view = kvp.Value;

                writer.Write(view.ViewId);
                writer.Write(view.PrefabPath);

                view.OnNetSerialize(writer);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            int count = reader.ReadInt32();
            Debug.Log($"Recovery view count : {count}");

            for (int i=0; i<count; i++)
            {
                int viewId = reader.ReadInt32();
                string prefabPath = reader.ReadString();

                Debug.Log($"Recovery view : {viewId} {prefabPath}");

                // 이미 있는지 확인
                NetView view;
                _viewMap.TryGetValue(viewId, out view);
                if(view == null)
                {
                    // 없다면 생성
                    var resObj = Resources.Load(prefabPath);
                    Debug.Log($"recovery : {prefabPath}");
                    var gameObj = GameObject.Instantiate(resObj) as GameObject;

                    view = gameObj.GetComponent<NetView>();
                    view.ViewId = viewId;
                }

                view.PrefabPath = prefabPath;
                view.OnNetDeserialize(reader);
            }
        }
    }
}