namespace FeatherWorks.Pooling {
	public interface ISpawnable {
		void OnSpawning();
		void OnSpawned();
	}

	public interface IDespawnable {
		void OnDespawning();
		void OnDespawned();
	}
}