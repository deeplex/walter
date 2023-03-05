<script lang="ts">
	import {
		HeaderGlobalAction,
		HeaderNav,
		Loading
	} from 'carbon-components-svelte';
	import { Save, TrashCan } from 'carbon-icons-svelte';

	import { WalterAnhaenge, WalterHeader } from '$WalterComponents';
	import { walter_delete, walter_put } from '$WalterServices/requests';

	export let title: Promise<string> | string = 'Saverwalter';
	export let a: any; // TODO replace with type that has anhaenge
	export let url: string;

	function click_save() {
		walter_put(url, a);
	}

	function click_delete(title: string) {
		walter_delete(url, title);
	}
</script>

<WalterHeader {title}>
	{#await title then x}
		<HeaderNav>
			{#await a}
				<HeaderGlobalAction>
					<Loading style="margin-left: 33%" withOverlay={false} small />
				</HeaderGlobalAction>
				<HeaderGlobalAction>
					<Loading style="margin-left: 33%" withOverlay={false} small />
				</HeaderGlobalAction>
			{:then}
				<HeaderGlobalAction on:click={click_save} icon={Save} />
				<HeaderGlobalAction on:click={() => click_delete(x)} icon={TrashCan} />
			{/await}
		</HeaderNav>
		{#await a then x}
			{#if x.anhaenge}
				<WalterAnhaenge rows={a.anhaenge} />
			{/if}
		{/await}
	{/await}
</WalterHeader>
