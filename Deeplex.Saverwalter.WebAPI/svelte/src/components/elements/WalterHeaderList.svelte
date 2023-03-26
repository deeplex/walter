<script lang="ts">
	import { Add } from 'carbon-icons-svelte';
	import { HeaderGlobalAction, HeaderNav } from 'carbon-components-svelte';

	import { goto } from '$app/navigation';

	import { WalterHeader } from '$WalterComponents';
	import { page } from '$app/stores';

	export let title: Promise<string> | string = 'Saverwalter';
	export let nonav: boolean = false;

	function click_new() {
		if (nonav) {
			return;
		}
		goto(`${$page.url.pathname}/new`);
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
