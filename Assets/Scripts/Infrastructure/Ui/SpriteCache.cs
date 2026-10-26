using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CKR.Infrastructure.Ui
{
    /// <summary>
    /// Keyed store for sprites built at runtime from downloaded images.
    ///
    /// Repeated polling would otherwise rebuild the very same icon over and over, leaking one
    /// <see cref="Texture2D"/> per download: textures are native objects and the GC never
    /// reclaims them.
    /// </summary>
    public interface ISpriteCache
    {
        bool TryGet(string key, out Sprite sprite);

        /// <summary>
        /// Takes ownership of the sprite and returns the instance that is actually cached:
        /// if another request already stored this key, the surplus sprite is destroyed and the
        /// existing one is returned, so callers can never end up holding a leaked texture.
        /// </summary>
        Sprite Put(string key, Sprite sprite);
    }

    public sealed class SpriteCache : ISpriteCache, IDisposable
    {
        private const int Capacity = 32;

        private readonly Dictionary<string, Sprite> _sprites = new Dictionary<string, Sprite>();
        private readonly Queue<string> _insertionOrder = new Queue<string>();

        private bool _isDisposed;

        public bool TryGet(string key, out Sprite sprite)
        {
            sprite = null;

            if (string.IsNullOrEmpty(key) || _isDisposed)
                return false;

            if (!_sprites.TryGetValue(key, out var cached))
                return false;

            // A cached sprite can still be destroyed from the outside; drop such entries.
            if (!cached)
            {
                _sprites.Remove(key);
                return false;
            }

            sprite = cached;
            return true;
        }

        public Sprite Put(string key, Sprite sprite)
        {
            if (!sprite)
                return null;

            if (string.IsNullOrEmpty(key) || _isDisposed)
            {
                Destroy(sprite);
                return null;
            }

            if (_sprites.TryGetValue(key, out var existing) && existing != null)
            {
                Destroy(sprite);
                return existing;
            }

            _sprites[key] = sprite;
            _insertionOrder.Enqueue(key);

            EvictIfNeeded();
            return sprite;
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            foreach (var entry in _sprites)
                Destroy(entry.Value);

            _sprites.Clear();
            _insertionOrder.Clear();
        }

        private void EvictIfNeeded()
        {
            while (_insertionOrder.Count > Capacity)
            {
                var oldestKey = _insertionOrder.Dequeue();

                if (!_sprites.TryGetValue(oldestKey, out var sprite))
                    continue;

                _sprites.Remove(oldestKey);
                Destroy(sprite);
            }
        }

        private static void Destroy(Sprite sprite)
        {
            if (!sprite)
                return;

            // The texture is owned by the sprite here: it was created by the download request
            // and is not shared with any asset.
            var texture = sprite.texture;

            Object.Destroy(sprite);

            if (texture)
                Object.Destroy(texture);
        }
    }
}
