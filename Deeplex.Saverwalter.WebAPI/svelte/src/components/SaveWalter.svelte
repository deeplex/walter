<script lang="ts">
	import {
		HeaderGlobalAction,
		HeaderNav,
		Loading
	} from 'carbon-components-svelte';
	import { Save } from 'carbon-icons-svelte';

	import { walter_put } from '$services/utils';

	export let a: Promise<any>;
	export let body: any;
	export let url: string;

	function click() {
		walter_put(url, body);
	}
</script>

<HeaderNav>
	{#await a}
		<HeaderGlobalAction>
			<Loading style="margin-left: 33%" withOverlay={false} small />
		</HeaderGlobalAction>
	{:then}
		<HeaderGlobalAction on:click={click} icon={Save} />
	{/await}
</HeaderNav>
