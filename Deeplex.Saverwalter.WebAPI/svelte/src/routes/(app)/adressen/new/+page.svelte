<script lang="ts">
	import {
		WalterAdresse,
		WalterGrid,
		WalterHeaderNew
	} from '$WalterComponents';
	import type { WalterAdresseEntry } from '$WalterLib';

	const apiURL = `/api/adressen`;
	const title = 'Neue Adresse';

	const entry: Partial<WalterAdresseEntry> = {};
</script>

<WalterHeaderNew {apiURL} {entry} {title} />

<WalterGrid>
	<WalterAdresse value={entry} />
</WalterGrid>
