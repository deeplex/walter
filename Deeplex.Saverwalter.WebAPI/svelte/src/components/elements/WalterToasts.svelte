<script lang="ts">
	import { ToastNotification } from 'carbon-components-svelte';
	import { fly } from 'svelte/transition';

	import { removeToast, toasts } from '$WalterStore';
	import type { WalterToast } from '$WalterTypes';

	function close(e: CustomEvent, index: number) {
		e.preventDefault();
		removeToast(index);
	}

	let toaster: Partial<WalterToast>[] = [];

	toasts.subscribe((value) => {
		toaster = value;
	});
</script>

<div
	style="; overflow: hidden; position: absolute; top: 48px; right: 0; z-index: 99"
>
	{#each toaster as toast, i}
		<div transition:fly={{ x: 200, duration: 200 }}>
			<ToastNotification
				on:close={(e) => close(e, i)}
				title={toast.title}
				kind={toast.kind}
				subtitle={toast.subtitle}
				caption={new Date().toLocaleString('de-DE')}
			/>
		</div>
	{/each}
</div>
