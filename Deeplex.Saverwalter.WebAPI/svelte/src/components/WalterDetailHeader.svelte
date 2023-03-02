<script lang="ts">
	import { Anhaenge } from '$components';
	import { walter_put } from '$services/utils';
	import {
		Header,
		HeaderGlobalAction,
		HeaderNav,
		HeaderUtilities,
		Loading
	} from 'carbon-components-svelte';
	import { Save } from 'carbon-icons-svelte';

	export let title: Promise<string> | string = 'Saverwalter';
	export let a: Promise<any>; // TODO replace with type that has anhaenge
	export let url: string;
	export let entry: any;

	const company = 'SaverWalter | ';

	function click() {
		walter_put(url, entry);
	}
</script>

{#await title}
	<Header href="/" {company} platformName="Lade...">
		<Loading withOverlay={false} small />
	</Header>
{:then x}
	<Header href="/" {company} platformName={x}>
		<HeaderUtilities>
			<HeaderNav>
				{#await a}
					<HeaderGlobalAction>
						<Loading style="margin-left: 33%" withOverlay={false} small />
					</HeaderGlobalAction>
				{:then}
					<HeaderGlobalAction on:click={click} icon={Save} />
				{/await}
			</HeaderNav>
			<Anhaenge rows={a.then((x) => x.anhaenge)} />
			<slot />
		</HeaderUtilities>
	</Header>
{/await}
