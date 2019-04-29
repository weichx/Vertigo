using System;

namespace Vertigo {

    public struct ShineEffectData : IEquatable<ShineEffectData> {

        public bool Equals(ShineEffectData other) {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ShineEffectData other && Equals(other);
        }

        public override int GetHashCode() {
            throw new NotImplementedException();
        }

    }

}