<script lang="ts">
	import {
		WalterGrid,
		WalterHeaderNew,
		WalterBetriebskostenrechnung
	} from '$WalterComponents';
	import type { WalterBetriebskostenrechnungEntry } from '$WalterTypes';
	import type { PageData } from './$types';

	export let data: PageData;

	const entry: Partial<WalterBetriebskostenrechnungEntry> = {};
</script>

<WalterHeaderNew apiURL={data.apiURL} {entry} title={data.title} />

<WalterGrid>
	<WalterBetriebskostenrechnung
		betriebskostentypen={data.betriebskostentypen}
		umlagen={data.umlagen}
		a={entry}
	/>
</WalterGrid>
