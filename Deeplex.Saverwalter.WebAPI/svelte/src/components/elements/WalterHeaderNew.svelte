<script lang="ts">
	import { HeaderGlobalAction, HeaderNav } from 'carbon-components-svelte';
	import { Save } from 'carbon-icons-svelte';

	import { WalterHeader } from '$WalterComponents';
	import { walter_post } from '$WalterServices/requests';
	import { goto } from '$app/navigation';

	export let title: string = 'Neu...';
	export let url: string;
	export let entry: any;

	async function click_post() {
		const j = await walter_post(url, entry);
		if (j.id) {
			goto(`${url}/${j.id}`.replace('api/', ''));
		}
	}
</script>

<WalterHeader {title}>
	<slot />
	<HeaderNav>
		<HeaderGlobalAction on:click={click_post} icon={Save} />
	</HeaderNav>
</WalterHeader>
