<script lang="ts">
	import {
		HeaderGlobalAction,
		HeaderNav,
		Loading
	} from 'carbon-components-svelte';
	import { Save, TrashCan } from 'carbon-icons-svelte';

	import { Anhaenge, WalterHeader } from '$components';
	import { walter_delete, walter_put } from '$services/requests';

	export let title: Promise<string> | string = 'Saverwalter';
	export let a: Promise<any>; // TODO replace with type that has anhaenge
	export let url: string;
	export let entry: any;

	function click_save() {
		walter_put(url, entry);
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
			{:then}
				<HeaderGlobalAction on:click={click_save} icon={Save} />
			{/await}
		</HeaderNav>
		<HeaderNav>
			{#await a}
				<HeaderGlobalAction>
					<Loading style="margin-left: 33%" withOverlay={false} small />
				</HeaderGlobalAction>
			{:then}
				<HeaderGlobalAction on:click={() => click_delete(x)} icon={TrashCan} />
			{/await}
		</HeaderNav>
		<Anhaenge rows={a.then((x) => x.anhaenge)} />
	{/await}
</WalterHeader>
