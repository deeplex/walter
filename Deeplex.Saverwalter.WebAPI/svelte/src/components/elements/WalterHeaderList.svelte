<script lang="ts">
	import { Add } from 'carbon-icons-svelte';
	import { HeaderGlobalAction, HeaderNav } from 'carbon-components-svelte';

	import { goto } from '$app/navigation';

	import { WalterHeader } from '$WalterComponents';

	export let title: Promise<string> | string = 'Saverwalter';
	export let apiUrl: string;
	export let nonav: boolean = false;

	function click_new() {
		if (nonav) {
			return;
		}
		// TODO apiURL just to replace it...
		goto(`${apiUrl}/new`.replace('api/', ''));
	}
</script>

<WalterHeader {title}>
	{#if !nonav}
		{#await title then}
			<HeaderNav>
				<HeaderGlobalAction on:click={click_new} icon={Add} />
			</HeaderNav>
		{/await}
	{/if}
</WalterHeader>
