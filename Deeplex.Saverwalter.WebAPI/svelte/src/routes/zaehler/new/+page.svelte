<script lang="ts">
	import {
		WalterGrid,
		WalterZaehler,
		WalterHeaderNew
	} from '$WalterComponents';
	import type { WalterZaehlerEntry } from '$WalterTypes';

	const url = `/api/zaehler`;
	const entry: Partial<WalterZaehlerEntry> = {};
	const title = 'Neuer Zähler';
</script>

<WalterHeaderNew {url} {entry} {title} />

<WalterGrid>
	<WalterZaehler {entry} />
</WalterGrid>
