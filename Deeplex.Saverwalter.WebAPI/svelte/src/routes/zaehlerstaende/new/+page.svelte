<script lang="ts">
	import {
		WalterGrid,
		WalterHeaderNew,
		WalterZaehlerstand
	} from '$WalterComponents';
	import type { WalterZaehlerstandEntry } from '$WalterTypes';

	const url = `/api/zaehlerstaende`;
	const title = 'Neuer Zählerstand';

	const entry: Partial<WalterZaehlerstandEntry> = {};
</script>

<WalterHeaderNew {url} {entry} {title} />

<WalterGrid>
	<WalterZaehlerstand {entry} />
</WalterGrid>
