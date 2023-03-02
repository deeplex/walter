<script lang="ts">
	import {
		WalterGrid,
		WalterHeaderNew,
		WalterBetriebskostenrechnung
	} from '$components';
	import type { BetriebskostenrechnungEntry } from '$types';

	const url = `/api/betriebskostenrechnungen`;
	const title = 'Neue Betriebskostenrechnung';

	const entry: Partial<BetriebskostenrechnungEntry> = {};
</script>

<WalterHeaderNew {url} {entry} {title} />

<WalterGrid>
	<WalterBetriebskostenrechnung {entry} />
</WalterGrid>
