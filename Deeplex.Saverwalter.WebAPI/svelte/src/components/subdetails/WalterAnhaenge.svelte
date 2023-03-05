<script lang="ts">
	import {
		HeaderAction,
		HeaderPanelDivider,
		HeaderPanelLink,
		HeaderPanelLinks,
		Loading
	} from 'carbon-components-svelte';

	import type { WalterAnhangEntry } from '$WalterTypes';

	export let rows: WalterAnhangEntry[];
</script>

{#await rows}
	<HeaderAction disabled>
		<svelte:fragment slot="text">
			<Loading style="margin-left: 1em;" withOverlay={false} small />
		</svelte:fragment>
	</HeaderAction>
{:then x}
	<HeaderAction text="({x.length})">
		<HeaderPanelLinks>
			<HeaderPanelDivider>Dateien ({x.length})</HeaderPanelDivider>
			{#each x as row}
				<HeaderPanelLink>{row.fileName}</HeaderPanelLink>
			{/each}
		</HeaderPanelLinks>
	</HeaderAction>
{/await}
