<script lang="ts">
	import { ToastNotification } from 'carbon-components-svelte';
	import { fly } from 'svelte/transition';

	import { removeToast, toasts } from '$WalterStore';
	import type { WalterToast } from '$WalterTypes';

	function close(index: number) {
		removeToast(index);
	}

	let toaster: Partial<WalterToast>[] = [];

	toasts.subscribe((value) => {
		toaster = value;
	});
</script>

{#each toaster as toast, i}
	<span in:fly={{ y: -300 }}>
		<ToastNotification
			onclose={() => close(i)}
			title={toast.title}
			kind={toast.kind}
			subtitle={toast.subtitle}
			caption={new Date().toLocaleString('de-DE')}
		/>
	</span>
{/each}
