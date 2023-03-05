<script lang="ts">
	import {
		WalterGrid,
		WalterHeaderNew,
		WalterBetriebskostenrechnung
	} from '$WalterComponents';
	import type { WalterBetriebskostenrechnungEntry } from '$WalterTypes';

	const url = `/api/betriebskostenrechnungen`;
	const title = 'Neue Betriebskostenrechnung';

	const entry: Partial<WalterBetriebskostenrechnungEntry> = {};
</script>

<WalterHeaderNew {url} {entry} {title} />

<WalterGrid>
	<WalterBetriebskostenrechnung a={entry} />
</WalterGrid>
