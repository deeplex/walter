<script lang="ts">
	import {
		HeaderAction,
		HeaderPanelDivider,
		HeaderPanelLink,
		HeaderPanelLinks,
		HeaderUtilities,
		Loading
	} from 'carbon-components-svelte';
	import { FileStorage, Tools } from 'carbon-icons-svelte';
	import type { AnhangListEntry } from '../types/anhanglist.type';

	let isOpen: boolean = false;

	export let rows: Promise<AnhangListEntry[]>;
</script>

{#await rows}
	<HeaderAction disabled>
		<svelte:fragment slot="text">
			<Loading style="margin-left: 1em;" withOverlay={false} small />
		</svelte:fragment>
	</HeaderAction>
{:then x}
	<HeaderUtilities>
		<HeaderAction isOpen text="({x.length})">
			<HeaderPanelLinks>
				<HeaderPanelDivider>Dateien ({x.length})</HeaderPanelDivider>
				{#each x as row}
					<HeaderPanelLink>{row.fileName}</HeaderPanelLink>
				{/each}
			</HeaderPanelLinks>
		</HeaderAction>
	</HeaderUtilities>
{/await}
