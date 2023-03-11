<script lang="ts">
	import {
		WalterAdresse,
		WalterGrid,
		WalterHeaderNew
	} from '$WalterComponents';
	import type { WalterAdresseEntry } from '$WalterTypes';

	const apiURL = `/api/adressen`;
	const title = 'Neue Adresse';

	const entry: Partial<WalterAdresseEntry> = {};
</script>

<WalterHeaderNew {apiURL} {entry} {title} />

<WalterGrid>
	<WalterAdresse value={entry} />
</WalterGrid>
