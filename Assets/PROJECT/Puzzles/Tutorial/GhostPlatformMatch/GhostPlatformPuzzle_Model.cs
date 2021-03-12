using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using Normal.Realtime.Serialization;

[RealtimeModel]
public partial class GhostPlatformPuzzle_Model
{
    [RealtimeProperty(1, true, true)]
    int _matchesLeftToWin;
}

/* ----- Begin Normal Autogenerated Code ----- */
public partial class GhostPlatformPuzzle_Model : RealtimeModel {
    public int matchesLeftToWin {
        get {
            return _cache.LookForValueInCache(_matchesLeftToWin, entry => entry.matchesLeftToWinSet, entry => entry.matchesLeftToWin);
        }
        set {
            if (this.matchesLeftToWin == value) return;
            _cache.UpdateLocalCache(entry => { entry.matchesLeftToWinSet = true; entry.matchesLeftToWin = value; return entry; });
            InvalidateReliableLength();
            FireMatchesLeftToWinDidChange(value);
        }
    }
    
    public delegate void PropertyChangedHandler<in T>(GhostPlatformPuzzle_Model model, T value);
    public event PropertyChangedHandler<int> matchesLeftToWinDidChange;
    
    private struct LocalCacheEntry {
        public bool matchesLeftToWinSet;
        public int matchesLeftToWin;
    }
    
    private LocalChangeCache<LocalCacheEntry> _cache = new LocalChangeCache<LocalCacheEntry>();
    
    public enum PropertyID : uint {
        MatchesLeftToWin = 1,
    }
    
    public GhostPlatformPuzzle_Model() : this(null) {
    }
    
    public GhostPlatformPuzzle_Model(RealtimeModel parent) : base(null, parent) {
    }
    
    protected override void OnParentReplaced(RealtimeModel previousParent, RealtimeModel currentParent) {
        UnsubscribeClearCacheCallback();
    }
    
    private void FireMatchesLeftToWinDidChange(int value) {
        try {
            matchesLeftToWinDidChange?.Invoke(this, value);
        } catch (System.Exception exception) {
            UnityEngine.Debug.LogException(exception);
        }
    }
    
    protected override int WriteLength(StreamContext context) {
        int length = 0;
        if (context.fullModel) {
            FlattenCache();
            length += WriteStream.WriteVarint32Length((uint)PropertyID.MatchesLeftToWin, (uint)_matchesLeftToWin);
        } else if (context.reliableChannel) {
            LocalCacheEntry entry = _cache.localCache;
            if (entry.matchesLeftToWinSet) {
                length += WriteStream.WriteVarint32Length((uint)PropertyID.MatchesLeftToWin, (uint)entry.matchesLeftToWin);
            }
        }
        return length;
    }
    
    protected override void Write(WriteStream stream, StreamContext context) {
        var didWriteProperties = false;
        
        if (context.fullModel) {
            stream.WriteVarint32((uint)PropertyID.MatchesLeftToWin, (uint)_matchesLeftToWin);
        } else if (context.reliableChannel) {
            LocalCacheEntry entry = _cache.localCache;
            if (entry.matchesLeftToWinSet) {
                _cache.PushLocalCacheToInflight(context.updateID);
                ClearCacheOnStreamCallback(context);
            }
            if (entry.matchesLeftToWinSet) {
                stream.WriteVarint32((uint)PropertyID.MatchesLeftToWin, (uint)entry.matchesLeftToWin);
                didWriteProperties = true;
            }
            
            if (didWriteProperties) InvalidateReliableLength();
        }
    }
    
    protected override void Read(ReadStream stream, StreamContext context) {
        while (stream.ReadNextPropertyID(out uint propertyID)) {
            switch (propertyID) {
                case (uint)PropertyID.MatchesLeftToWin: {
                    int previousValue = _matchesLeftToWin;
                    _matchesLeftToWin = (int)stream.ReadVarint32();
                    bool matchesLeftToWinExistsInChangeCache = _cache.ValueExistsInCache(entry => entry.matchesLeftToWinSet);
                    if (!matchesLeftToWinExistsInChangeCache && _matchesLeftToWin != previousValue) {
                        FireMatchesLeftToWinDidChange(_matchesLeftToWin);
                    }
                    break;
                }
                default: {
                    stream.SkipProperty();
                    break;
                }
            }
        }
    }
    
    #region Cache Operations
    
    private StreamEventDispatcher _streamEventDispatcher;
    
    private void FlattenCache() {
        _matchesLeftToWin = matchesLeftToWin;
        _cache.Clear();
    }
    
    private void ClearCache(uint updateID) {
        _cache.RemoveUpdateFromInflight(updateID);
    }
    
    private void ClearCacheOnStreamCallback(StreamContext context) {
        if (_streamEventDispatcher != context.dispatcher) {
            UnsubscribeClearCacheCallback(); // unsub from previous dispatcher
        }
        _streamEventDispatcher = context.dispatcher;
        _streamEventDispatcher.AddStreamCallback(context.updateID, ClearCache);
    }
    
    private void UnsubscribeClearCacheCallback() {
        if (_streamEventDispatcher != null) {
            _streamEventDispatcher.RemoveStreamCallback(ClearCache);
            _streamEventDispatcher = null;
        }
    }
    
    #endregion
}
/* ----- End Normal Autogenerated Code ----- */