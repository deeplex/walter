<script lang="ts">
	import {
		WalterBetriebskostenrechnungen,
		WalterHeaderList
	} from '$WalterComponents';
	import { walter_get } from '$WalterServices/requests';
	import type { WalterBetriebskostenrechnungEntry } from '$WalterTypes';

	const url = 'betriebskostenrechnungen';

	const rows: Promise<WalterBetriebskostenrechnungEntry[]> = walter_get(
		`/api/${url}`
	);
</script>

<WalterHeaderList {url} title="Betriebskostenrechnung" />
<WalterBetriebskostenrechnungen search {rows} />
