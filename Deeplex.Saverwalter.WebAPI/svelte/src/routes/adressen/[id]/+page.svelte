<script lang="ts">
	import {
		WalterAdresse,
		WalterGrid,
		WalterHeaderDetail,
		WalterWohnungen
	} from '$WalterComponents';
	import type { WalterWohnungEntry } from '$WalterTypes';
	import { Accordion } from 'carbon-components-svelte';
	import type { PageData } from './$types';

	export let data: PageData;
	const wohnungEntry: Partial<WalterWohnungEntry> = { adresse: { ...data.a } };
</script>

<WalterHeaderDetail a={data.a} url={data.url} title={data.a.anschrift} />

<WalterGrid>
	<WalterAdresse bind:value={data.a} />

	<Accordion>
		<WalterWohnungen
			kontakte={data.kontakte}
			a={wohnungEntry}
			title="Wohnungen"
			rows={data.a.wohnungen}
		/>
		<!-- TODO add Kontakte, Verträge, Umlagen, Zähler? -->
	</Accordion>
</WalterGrid>
