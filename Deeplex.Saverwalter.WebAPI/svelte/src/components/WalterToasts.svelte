<script lang="ts">
	import { ToastNotification } from 'carbon-components-svelte';
	import { fly } from 'svelte/transition';

	import { removeToast, toasts } from '$store';
	import type { WalterToast } from '$types';

	function close(e: CustomEvent<any>) {
		removeToast(e.detail);
	}

	let toaster: Partial<WalterToast>[] = [];

	toasts.subscribe((value) => {
		toaster = value;
	});
</script>

{#each toaster as toast}
	<span in:fly={{ y: -300 }}>
		<ToastNotification
			on:close={close}
			title={toast.title}
			kind={toast.kind}
			subtitle={toast.subtitle}
			caption={new Date().toLocaleString('de-DE')}
		/>
	</span>
{/each}
<!-- 
<ToastNotification
	title="Error"
	subtitle="An internal server error occurred."
	caption={new Date().toLocaleString('de-DE')}
/> -->
