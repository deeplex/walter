<script lang="ts">
	import { Betriebskostenrechnungen, WalterHeader } from '$components';
	import { walter_get } from '$services/utils';
	import type { BetriebskostenrechnungEntry } from '$types';

	const rows: Promise<BetriebskostenrechnungEntry[]> = walter_get(
		'/api/betriebskostenrechnungen'
	);
</script>

<WalterHeader title="Betriebskostenrechnung" />
<Betriebskostenrechnungen search {rows} />
