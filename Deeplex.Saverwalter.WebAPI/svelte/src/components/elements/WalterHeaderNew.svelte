<script lang="ts">
	import { HeaderGlobalAction, HeaderNav } from 'carbon-components-svelte';
	import { Save } from 'carbon-icons-svelte';

	import { WalterHeader } from '$components';
	import { walter_post } from '$services/requests';

	export let title: string = 'Neu...';
	export let url: string;
	export let entry: any;

	function click_post() {
		walter_post(url, entry);
	}
</script>

<WalterHeader {title}>
	<slot />
	<HeaderNav>
		<HeaderGlobalAction on:click={click_post} icon={Save} />
	</HeaderNav>
</WalterHeader>
