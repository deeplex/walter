<script lang="ts">
	import { Betriebskostenrechnungen, WalterHeaderList } from '$components';
	import { walter_get } from '$services/utils';
	import type { BetriebskostenrechnungEntry } from '$types';

	const url = 'betriebskostenrechnungen';

	const rows: Promise<BetriebskostenrechnungEntry[]> = walter_get(
		`/api/${url}`
	);
</script>

<WalterHeaderList {url} title="Betriebskostenrechnung" />
<Betriebskostenrechnungen search {rows} />
