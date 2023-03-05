<script lang="ts">
	import { Add } from 'carbon-icons-svelte';
	import { HeaderGlobalAction, HeaderNav } from 'carbon-components-svelte';

	import { goto } from '$app/navigation';

	import { WalterHeader } from '$WalterComponents';

	export let title: Promise<string> | string = 'Saverwalter';
	export let url: string;

	function click_new() {
		goto(`${url}/new`.replace('api/', ''));
	}
</script>

<WalterHeader {title}>
	{#await title then}
		<HeaderNav>
			<HeaderGlobalAction on:click={click_new} icon={Add} />
		</HeaderNav>
	{/await}
</WalterHeader>
