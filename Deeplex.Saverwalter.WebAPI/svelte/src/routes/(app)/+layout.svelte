<script lang="ts">
	// import 'carbon-components-svelte/css/white.css';
	import 'carbon-components-svelte/css/all.css';

	import { WalterSideNav } from '$WalterComponents';
	import { Modal } from 'carbon-components-svelte';
	import type { WalterModalControl } from '$WalterTypes';
	import { walterModalControl } from '$WalterStore';
	import { getAccessToken } from '$WalterServices/auth';
	import { goto } from '$app/navigation';
	if (getAccessToken() == null) {
		goto('/login');
	}

	let modalControl: WalterModalControl;
	walterModalControl.subscribe((value) => {
		modalControl = value;
	});
</script>

<WalterSideNav />
<Modal
	{...modalControl}
	bind:open={modalControl.open}
	secondaryButtonText="Abbrechen"
	on:click:button--secondary={() => (modalControl.open = false)}
	on:click:button--primary={() => (modalControl.open = false)}
	on:open
	on:close
	on:submit={modalControl.submit}
>
	<p>{modalControl.content}</p>
</Modal>
<slot />
