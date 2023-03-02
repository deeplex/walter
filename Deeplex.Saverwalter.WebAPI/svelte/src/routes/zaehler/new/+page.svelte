<script lang="ts">
	import { WalterGrid, WalterZaehler, WalterHeaderNew } from '$components';
	import type { ZaehlerEntry } from '$types';

	const url = `/api/zaehler`;
	const entry: Partial<ZaehlerEntry> = {};
	const title = 'Neuer Zähler';
</script>

<WalterHeaderNew {url} {entry} {title} />

<WalterGrid>
	<WalterZaehler {entry} />
</WalterGrid>
