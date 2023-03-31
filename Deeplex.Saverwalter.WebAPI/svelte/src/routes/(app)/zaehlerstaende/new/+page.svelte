<script lang="ts">
	import {
		WalterGrid,
		WalterHeaderNew,
		WalterZaehlerstand
	} from '$WalterComponents';
	import type { WalterZaehlerstandEntry } from '$WalterLib';

	const apiURL = `/api/zaehlerstaende`;
	const title = 'Neuer Zählerstand';

	const entry: Partial<WalterZaehlerstandEntry> = {};
</script>

<WalterHeaderNew {apiURL} {entry} {title} />

<WalterGrid>
	<WalterZaehlerstand a={entry} />
</WalterGrid>
